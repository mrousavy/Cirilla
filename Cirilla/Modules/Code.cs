using Cirilla.Services.Roslyn;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Code : ModuleBase {
        [Command("exec"), Summary("Execute or run C# Code/Scripts")]
        public async Task Execute([Summary("The Code to execute")] [Remainder] string code) {
            //Requirements to execute C# Roslyn scripts
            if (!(Context.User is IGuildUser user) || !user.GuildPermissions.KickMembers) {
                await ReplyAsync("You're not allowed to use this super premium command!");
                return;
            }
            if (!Information.AllowScripts) {
                await ReplyAsync("C# Roslyn Scripts are disabled via Config.");
                return;
            }

            IUserMessage tempMessage = await ReplyAsync("Compiling.. :inbox_tray:");

            Embed embed = await ScriptRunnerService.ScriptEmbed(code, Context.User);

            await tempMessage.DeleteAsync();

            await ReplyAsync("", embed: embed);
        }
    }
}
