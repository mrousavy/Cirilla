using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Connection : ModuleBase {
        [Command("ping"), Summary("Test Connection")]
        public async Task Ping() {
            await ReplyAsync($"Pong! :ping_pong:" + Environment.NewLine + $"{Cirilla.Client.Latency}ms");
        }
    }
}
