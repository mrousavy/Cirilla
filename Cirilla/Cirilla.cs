using Cirilla.Services.Permissions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Cirilla {
    public class Cirilla {
        #region Privates

        private readonly CommandService _service;

        #endregion

        #region Publics

        public bool StopRequested { get; set; }
        public static DiscordSocketClient Client;

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
            Client.JoinedGuild += GuildInfoReceived;
            Client.GuildAvailable += GuildInfoReceived;

            CommandServiceConfig serviceConfig = new CommandServiceConfig {
                CaseSensitiveCommands = false,
                SeparatorChar = '$',
                LogLevel = logSeverity
            };
            _service = new CommandService(serviceConfig);
            _service.Log += Log;
            _service.AddModulesAsync(Assembly.GetEntryAssembly()).GetAwaiter().GetResult();

            try {
                Login().GetAwaiter().GetResult();
            } catch (Exception ex) {
                ConsoleHelper.Log($"Could not login as Discord Bot! {ex.Message}", LogSeverity.Critical);
            }
        }

        private static Task GuildInfoReceived(SocketGuild arg) {
            try {
                PermissionValidator.ValidatePermission(arg);
            } catch (Exception ex) {
                ConsoleHelper.Log($"Could not validate Permissions for Discord Guild \"{arg.Name}\"! {ex.Message}",
                    LogSeverity.Error);
            }
            return Task.CompletedTask;
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

                if (messageArg.Channel is IGuildChannel guildchannel) {
                    IEmote emote = Modules.RandomEmote.GetRandomEmote(guildchannel.Guild);
                    if (emote != null) {
                        await message.AddReactionAsync(emote);
                        await ConsoleHelper.Log("Added random Emote to a message!", LogSeverity.Info);
                    }
                }

                // Command Begin
                int argPos = 0;
                // Determine if the message is a command
                if (!(message.HasCharPrefix(Information.Prefix, ref argPos) ||
                      message.HasStringPrefix(Information.SecondaryPrefix, ref argPos) ||
                      message.HasMentionPrefix(Client.CurrentUser, ref argPos)))
                    return;
                CommandContext context = new CommandContext(Client, message);
                IResult result = await _service.ExecuteAsync(context, argPos);
                if (!result.IsSuccess) {
                    await ConsoleHelper.Log($"Command did not execute correctly! {result.ErrorReason}",
                        LogSeverity.Error);
                    //await context.Channel.SendMessageAsync(result.ErrorReason);

                    Embed embed = Helper.WrongCommand(message, _service, context);
                    if (embed == null) {
                        await context.Channel.SendMessageAsync("Pardon?");
                    } else {
                        await context.Channel.SendMessageAsync("", embed: embed);
                    }
                }
            }
        }

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