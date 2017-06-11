using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Admin : ModuleBase {
        [Command("prefix"), Summary("Change prefix")]
        public async Task ChangePrefix([Summary("New prefix")] string prefix) {
            try {
                IGuildUser user = Context.User as IGuildUser;
                if (user == null) {
                    return;
                }
                if (!user.GuildPermissions.Has(GuildPermission.ManageMessages)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                string before = Information.SecondaryPrefix;
                Information.Config.SecondaryPrefix = prefix;
                await ReplyAsync($"Prefix changed from `{before}` to `{prefix}`!");
                Information.SaveInfo();
            } catch {
                await ReplyAsync("Whoops, unfortunately I couldn't change the prefix.. :confused:");
            }
        }
    }
}
