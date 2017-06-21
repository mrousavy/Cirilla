using Discord.Commands;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Greetings : ModuleBase {
        [Command("hi"), Summary("Greets the User")]
        public async Task Hi() {
            await ReplyAsync("Hi! :wave: How are you doing?"); //ask user how he is doing (we don't care tho)
        }

        [Command("hello"), Summary("Greets the User")]
        public async Task Hello() {
            await ReplyAsync("Hi! :wave: How are you doing?"); //ask user how he is doing (we don't care tho)
        }
    }
}