using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Cirilla {
    public class Cirilla {
        #region Privates
        private DiscordSocketClient _client;
        #endregion

        #region Publics
        public bool IsDisposed { get; set; }
        #endregion

        public Cirilla() {
            DiscordSocketConfig config = new DiscordSocketConfig {
                LogLevel = LogSeverity.Info
            };
            _client = new DiscordSocketClient(config);
            _client.Log += Log;
            _client.MessageReceived += MessageReceived;

            Login().GetAwaiter().GetResult();
        }

        private Task MessageReceived(SocketMessage arg) {
            //TODO:

            return Task.CompletedTask;
        }

        public async Task Stop() {
            await _client.SetStatusAsync(UserStatus.Offline);
            await _client.StopAsync();
        }

        public async Task Login() {
            await _client.LoginAsync(TokenType.Bot, Information.Token);
            await _client.StartAsync();
        }

        private Task Log(LogMessage message) {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}
