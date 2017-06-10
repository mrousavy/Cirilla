using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Cirilla {
    public class Cirilla {
        #region Privates
        private DiscordSocketClient _client;
        private CommandService _service;
        #endregion

        #region Publics
        public bool IsDisposed { get; set; }
        #endregion

        public Cirilla(LogSeverity logSeverity) {
            DiscordSocketConfig config = new DiscordSocketConfig {
                LogLevel = logSeverity
            };
            _client = new DiscordSocketClient(config);
            _client.Log += Log;
            _client.MessageReceived += MessageReceived;
            _client.UserJoined += EventHelper.UserJoined;
            _client.UserLeft += EventHelper.UserLeft;

            CommandServiceConfig serviceConfig = new CommandServiceConfig {
                CaseSensitiveCommands = false,
                SeparatorChar = '$',
                LogLevel = logSeverity
            };
            _service = new CommandService(serviceConfig);
            _service.Log += Log;
            _service.AddModulesAsync(Assembly.GetEntryAssembly()).GetAwaiter().GetResult();

            Login().GetAwaiter().GetResult();

            Timer timer = new Timer(Modules.Xp.TimerCallback, _client, Information.XpGiveInterval, Information.XpGiveInterval);
        }

        private async Task MessageReceived(SocketMessage messageArg) {
            // Don't process the command if it was a System Message
            SocketUserMessage message = messageArg as SocketUserMessage;
            if (message == null)
                return;

            if (message.Author.ToString() != _client.CurrentUser.ToString()) {
                await ConsoleHelper.Log(new LogMessage(
                    LogSeverity.Info,
                    message.Author.ToString(),
                    message.Content));

                // Command Begin
                int argPos = 0;
                // Determine if the message is a command
                if (!(message.HasCharPrefix(Information.Prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                    return;
                CommandContext context = new CommandContext(_client, message);
                IResult result = await _service.ExecuteAsync(context, argPos);
                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        public async Task Stop() {
            if (_client.ConnectionState == ConnectionState.Connected)
                await _client.SetStatusAsync(UserStatus.Offline);
            if (_client.ConnectionState != ConnectionState.Disconnecting &&
                _client.ConnectionState != ConnectionState.Disconnected)
                await _client.StopAsync();
        }

        public async Task Login() {
            await _client.LoginAsync(TokenType.Bot, Information.Token);
            await _client.StartAsync();
        }

        private Task Log(LogMessage message) {
            ConsoleHelper.Log(message);
            return Task.CompletedTask;
        }
    }
}
