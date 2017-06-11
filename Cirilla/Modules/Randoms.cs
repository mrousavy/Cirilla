using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Randoms : ModuleBase {
        [Command("flip"), Summary("Flip a Coin!")]
        public async Task Flip() {
            int result = new Random().Next(0, 2);
            await ReplyAsync(result == 0 ? "Head!" : "Tails!");
        }


        [Command("random"), Summary("Generate a Random number")]
        public async Task Random([Summary("Maximum Value")] int maximum) {
            int result = new Random().Next(0, maximum) + 1;
            await ReplyAsync(result.ToString());
        }
    }
}
