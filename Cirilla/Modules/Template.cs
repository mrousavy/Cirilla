using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Template : ModuleBase {
        [Command("Test"), Summary("Test")]
        public async Task Command([Summary("")] double num) {
            try {
                await ReplyAsync($"");
            } catch (Exception ex) {
                await ReplyAsync($"Error! ({ex.Message})");
            }
        }
    }
}
