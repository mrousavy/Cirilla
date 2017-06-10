using Discord.Commands;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Greetings : ModuleBase {
        [Command("hi"), Summary("Greets the User")]
        public async Task Hi() {
            await ReplyAsync($"Hi! How are you doing?");
        }

        [Command("hello"), Summary("Greets the User")]
        public async Task Hello() {
            await ReplyAsync($"Hi! How are you doing?");
        }

        [Command("hallo"), Summary("Greets the User")]
        public async Task Hallo() {
            await ReplyAsync($"Hi! How are you doing?");
        }

        [Command("seas"), Summary("Greets the User")]
        public async Task Seas() {
            await ReplyAsync($"Seas hawara");
        }

        [Command("servas"), Summary("Greets the User")]
        public async Task Servas() {
            await ReplyAsync($"Servas hawara");
        }

        [Command("sup"), Summary("Greets the User")]
        public async Task Sup() {
            await ReplyAsync($"Sup! How are you doing?");
        }
    }
}
