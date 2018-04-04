using System.Threading.Tasks;
using Discord.Commands;

namespace Cirilla.Modules
{
    public class Connection : ModuleBase
    {
        [Command("ping")]
        [Summary("Test Connection")]
        public async Task Ping()
        {
            await ReplyAsync($"Pong! :ping_pong: ({Cirilla.Client.Latency}ms)");
        }
    }
}