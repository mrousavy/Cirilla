using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Cirilla {
    public class Cirilla {
        #region Privates
        private DiscordSocketClient _client;
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
            CommandService service = new CommandService(serviceConfig);
            service.Log += Log;

            Login().GetAwaiter().GetResult();
        }

        private async Task MessageReceived(SocketMessage message) {
            //TODO:
            if (message.Author.ToString() != _client.CurrentUser.ToString())
                await ConsoleHelper.Log(new LogMessage(
                    LogSeverity.Info,
                    message.Author.ToString(),
                    message.Content));
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
