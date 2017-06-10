using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Cirilla {
    public class Cirilla : IDisposable {
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

            Login().GetAwaiter().GetResult();
        }

        public void Dispose() {
            _client.LogoutAsync().GetAwaiter().GetResult();
            _client.Dispose();
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
