using Discord.Commands;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Greetings : ModuleBase {
        [Command("hi"), Summary("Greets the User")]
        public async Task Hi() {
            await ReplyAsync($"Hi! :wave: How are you doing?");
        }

        [Command("hello"), Summary("Greets the User")]
        public async Task Hello() {
            await ReplyAsync($"Hi! :wave: How are you doing?");
        }

        [Command("hallo"), Summary("Greets the User")]
        public async Task Hallo() {
            await ReplyAsync($"Hi! :wave: How are you doing?");
        }

        [Command("seas"), Summary("Greets the User")]
        public async Task Seas() {
            await ReplyAsync($"Seas hawara :wave:");
        }

        [Command("servas"), Summary("Greets the User")]
        public async Task Servas() {
            await ReplyAsync($"Servas hawara :wave:");
        }

        [Command("sup"), Summary("Greets the User")]
        public async Task Sup() {
            await ReplyAsync($"Sup! :wave: How are you doing?");
        }
    }
}
