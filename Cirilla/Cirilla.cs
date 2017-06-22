using Cirilla.Services.GuildConfig;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Cirilla {
    public class Cirilla {
        #region Privates

        private CommandService Service { get; }

        #endregion

        #region Publics

        public bool StopRequested { get; set; }
        public static DiscordSocketClient Client { get; set; }

        #endregion

        public Cirilla(LogSeverity logSeverity) {
            DiscordSocketConfig config = new DiscordSocketConfig {
                LogLevel = logSeverity,
                WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance
            };
            //Client = new WS4NetClient();
            Client = new DiscordSocketClient(config);
            Client.Log += Log;
            Client.MessageReceived += MessageReceived;
            Client.UserJoined += EventHelper.UserJoined;
            Client.UserLeft += EventHelper.UserLeft;
            Client.LeftGuild += EventHelper.LeftGuild;
            Client.JoinedGuild += EventHelper.JoinedGuild;
            Client.GuildAvailable += EventHelper.GuildAvailable;
            Client.Ready += EventHelper.Ready;

            CommandServiceConfig serviceConfig = new CommandServiceConfig {
                CaseSensitiveCommands = false,
                SeparatorChar = '$',
                LogLevel = logSeverity
            };
            Service = new CommandService(serviceConfig);
            Service.Log += Log;
            Service.AddModulesAsync(Assembly.GetEntryAssembly()).GetAwaiter().GetResult();

            try {
                Login().GetAwaiter().GetResult();
            } catch (Exception ex) {
                ConsoleHelper.Log($"Could not login as Discord Bot! {ex.Message}", LogSeverity.Critical);
            }
        }

        private async Task MessageReceived(SocketMessage messageArg) {
            // Don't process the command if it was a System Message
            SocketUserMessage message = messageArg as SocketUserMessage;
            if (message == null)
                return;

            if (message.Author.ToString() != Client.CurrentUser.ToString()) {
                await ConsoleHelper.Log(new LogMessage(
                    LogSeverity.Info,
                    message.Author.ToString(),
                    message.Content));

                string secondaryPrefix;
                bool enablePrimary = true;
                string guildname = "private";

                if (messageArg.Channel is IGuildChannel guildchannel) {
                    guildname = guildchannel.Guild.Name;

                    GuildConfiguration config = GuildConfigManager.Get(guildchannel.Guild.Id);
                    secondaryPrefix = config.Prefix;
                    enablePrimary = config.EnablePrimaryPrefix;

                    IEmote emote = Modules.RandomEmote.GetRandomEmote(guildchannel.Guild);
                    if (emote != null) {
                        await message.AddReactionAsync(emote);
                        await ConsoleHelper.Log("Added random Emote to a message!", LogSeverity.Info);
                    }
                } else {
                    secondaryPrefix = Information.SecondaryPrefix;
                }


                // Command (after prefix) Begin
                int argPos = 0;

                // Determine if the message is a command by checking for all prefixes
                bool primaryMatch = enablePrimary && message.HasCharPrefix(Information.Prefix, ref argPos);
                bool secondaryMatch = message.HasStringPrefix(secondaryPrefix, ref argPos);
                bool mentionMatch = message.HasMentionPrefix(Client.CurrentUser, ref argPos);
                if (!(primaryMatch || secondaryMatch || mentionMatch))
                    return;

                CommandLogger.Log(message.Author.ToString(), guildname, message.Content);

                CommandContext context = new CommandContext(Client, message);
                IResult result = await Service.ExecuteAsync(context, argPos);
                if (!result.IsSuccess) {
                    await ConsoleHelper.Log($"Command did not execute correctly! {result.ErrorReason}",
                        LogSeverity.Error);
                    //await context.Channel.SendMessageAsync(result.ErrorReason);

                    //Find out what the user probably meant
                    Embed embed = Helper.WrongCommand(message, Service, context);
                    if (embed == null) {
                        //unknown command/overloads
	    //TODO: commented -> Bot is saying nothing on wrong command
                        // await context.Channel.SendMessageAsync("Pardon?");
                    } else {
                        //Send "did you mean this?:" message
                        await context.Channel.SendMessageAsync("", embed: embed);
                    }
                }
            }
        }

        //stop bot
        public async Task Stop() {
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

        public async Task Login() {
            await Client.LoginAsync(TokenType.Bot, Information.Token);
            await Client.StartAsync();
        }

        private static Task Log(LogMessage message) {
            ConsoleHelper.Log(message);
            return Task.CompletedTask;
        }
    }
}