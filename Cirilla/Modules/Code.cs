using Discord.Commands;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Code : ModuleBase {
        [Command("exec"), Summary("Execute or run C# Code/Scripts")]
        public async Task Execute() {
            //TODO:
            await ReplyAsync("Sorry, this is a work in progress!");
        }
    }
}
