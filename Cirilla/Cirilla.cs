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

            //TODO:
            Client.GuildAvailable += GuildInfoReceived;
            Client.GuildMembersDownloaded += GuildInfoReceived;

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

        private static Task GuildInfoReceived(SocketGuild arg) {
            try {
                PermissionValidator.ValidatePermissions(arg);
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

                // Command (after prefix) Begin
                int argPos = 0;
                // Determine if the message is a command by checking for all prefixes
                if (!(message.HasCharPrefix(Information.Prefix, ref argPos) ||
                      message.HasStringPrefix(Information.SecondaryPrefix, ref argPos) ||
                      message.HasMentionPrefix(Client.CurrentUser, ref argPos)))
                    return;
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
                        await context.Channel.SendMessageAsync("Pardon?");
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
