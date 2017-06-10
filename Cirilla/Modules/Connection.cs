using Discord.Commands;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Connection : ModuleBase {
        [Command("ping"), Summary("Test Connection")]
        public async Task Ping() {
            await ReplyAsync("Pong!");
        }
    }
}
