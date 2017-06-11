using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;

namespace Cirilla {
    public class Cirilla {
        #region Privates
        private CommandService _service;
        #endregion

        #region Publics
        public bool IsDisposed { get; set; }
        public static DiscordSocketClient Client;
        #endregion

        public Cirilla(LogSeverity logSeverity) {
            DiscordSocketConfig config = new DiscordSocketConfig {
                LogLevel = logSeverity
            };
            Client = new DiscordSocketClient(config);
            Client.Log += Log;
            Client.MessageReceived += MessageReceived;
            Client.UserJoined += EventHelper.UserJoined;
            Client.UserLeft += EventHelper.UserLeft;

            CommandServiceConfig serviceConfig = new CommandServiceConfig {
                CaseSensitiveCommands = false,
                SeparatorChar = '$',
                LogLevel = logSeverity
            };
            _service = new CommandService(serviceConfig);
            _service.Log += Log;
            _service.AddModulesAsync(Assembly.GetEntryAssembly()).GetAwaiter().GetResult();

            Login().GetAwaiter().GetResult();
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


                IEmote emote = Modules.RandomEmote.GetRandomEmote((messageArg.Channel as IGuildChannel).Guild, message);
                if (emote != null)
                    await message.AddReactionAsync(emote);

                // Command Begin
                int argPos = 0;
                // Determine if the message is a command
                if (!(message.HasCharPrefix(Information.Prefix, ref argPos) ||
                    message.HasCharPrefix(Information.SecondaryPrefix, ref argPos) ||
                    message.HasMentionPrefix(Client.CurrentUser, ref argPos)))
                    return;
                CommandContext context = new CommandContext(Client, message);
                IResult result = await _service.ExecuteAsync(context, argPos);
                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        public async Task Stop() {
            if (Client.ConnectionState == ConnectionState.Connected)
                await Client.SetStatusAsync(UserStatus.Offline);
            if (Client.ConnectionState != ConnectionState.Disconnecting &&
                Client.ConnectionState != ConnectionState.Disconnected)
                await Client.StopAsync();
        }

        public async Task Login() {
            await Client.LoginAsync(TokenType.Bot, Information.Token);
            await Client.StartAsync();
        }

        private Task Log(LogMessage message) {
            ConsoleHelper.Log(message);
            return Task.CompletedTask;
        }
    }
}
