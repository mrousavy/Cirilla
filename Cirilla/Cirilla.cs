﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Cirilla.Services.GuildConfig;
using Cirilla.Services.Reminder;
using Discord;
using Discord.Commands;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;

namespace Cirilla
{
    public class Cirilla
    {
        public Cirilla(LogSeverity logSeverity)
        {
            var config = new DiscordSocketConfig
            {
                LogLevel = logSeverity
            };
            if (Information.NeedsWs4Net)
                config.WebSocketProvider = WS4NetProvider.Instance;

            Client = new DiscordSocketClient(config);
            Client.Log += Log;
            Client.MessageReceived += MessageReceived;
            Client.MessageReceived += EventHelper.RandomEmoji;
            //Client.MessageReceived += EventHelper.QuestionThinking; annoying?
            Client.UserJoined += EventHelper.UserJoined;
            Client.UserLeft += EventHelper.UserLeft;
            Client.LeftGuild += EventHelper.LeftGuild;
            Client.JoinedGuild += EventHelper.JoinedGuild;
            Client.GuildAvailable += EventHelper.GuildAvailable;
            Client.Ready += EventHelper.Ready;

            var serviceConfig = new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                LogLevel = logSeverity
            };
            Service = new CommandService(serviceConfig);
            Service.Log += Log;
            Service.AddTypeReader(typeof(Timediff), new ReminderTypeReader());
            Service.AddModulesAsync(Assembly.GetEntryAssembly()).GetAwaiter().GetResult();

            Login().GetAwaiter().GetResult();
        }

        #region Privates

        private CommandService Service { get; }

        #endregion

        private async Task MessageReceived(SocketMessage messageArg)
        {
            // Don't process the command if it was a System Message
            if (!(messageArg is SocketUserMessage message))
                return;

            if (message.Author.ToString() != Client.CurrentUser.ToString())
            {
                string secondaryPrefix;
                bool enablePrimary = true;
                string guildname = "private";

                if (messageArg.Channel is IGuildChannel guildchannel)
                {
                    guildname = guildchannel.Guild.Name;

                    var config = GuildConfigManager.Get(guildchannel.Guild.Id);
                    secondaryPrefix = config.Prefix;
                    enablePrimary = config.EnablePrimaryPrefix;
                } else
                {
                    secondaryPrefix = Information.SecondaryPrefix;
                }

                //Log to console but don't write log to file
                ConsoleHelper.Log(new LogMessage(LogSeverity.Info, $"{message.Author}@{guildname}", message.Content),
                    false);

                // Command (after prefix) Begin
                int argPos = 0;

                // Determine if the message is a command by checking for all prefixes
                bool primaryMatch = enablePrimary && message.HasCharPrefix(Information.Prefix, ref argPos);
                bool secondaryMatch = message.HasStringPrefix(secondaryPrefix, ref argPos);
                bool mentionMatch = message.HasMentionPrefix(Client.CurrentUser, ref argPos);
                if (!(primaryMatch || secondaryMatch || mentionMatch))
                    return;

                CommandLogger.Log(message.Author.ToString(), guildname, message.Content);

                var context = new CommandContext(Client, message);
                var result = await Service.ExecuteAsync(context, argPos);
                if (!result.IsSuccess)
                {
                    ConsoleHelper.Log($"Command did not execute correctly! {result.ErrorReason}",
                        LogSeverity.Error);
                    //await context.Channel.SendMessageAsync(result.ErrorReason);

                    //Find out what the user probably meant
                    var embed = Helper.WrongCommand(message, Service, context);
                    if (embed != default(Embed))
                        await context.Channel.SendMessageAsync("", embed: embed);
                }
            }
        }

        //stop bot
        public async Task Stop()
        {
            if (StopRequested)
                return;
            StopRequested = true;

            if (Client.ConnectionState == ConnectionState.Connected)
                await Client.SetStatusAsync(UserStatus.Offline);
            if (Client.ConnectionState != ConnectionState.Disconnecting &&
                Client.ConnectionState != ConnectionState.Disconnected)
                await Client.StopAsync();
            //await Client.LogoutAsync();
            Console.WriteLine("stopped");
        }

        public async Task Login()
        {
            await Client.LoginAsync(TokenType.Bot, Information.Token);
            await Client.StartAsync();
        }

        private static Task Log(LogMessage message)
        {
            ConsoleHelper.Log(message);
            return Task.CompletedTask;
        }

        #region Publics

        public bool StopRequested { get; set; }
        public static DiscordSocketClient Client { get; set; }

        #endregion
    }
}