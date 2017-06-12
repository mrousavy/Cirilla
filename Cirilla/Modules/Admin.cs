using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Admin : ModuleBase {
        [Command("prefix"), Summary("Change prefix")]
        public async Task ChangePrefix([Summary("New prefix")] params string[] arguments) {
            try {
                IGuildUser user = Context.User as IGuildUser;
                if (user == null) {
                    return;
                }
                if (!user.GuildPermissions.Has(GuildPermission.ManageMessages)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                string prefix = string.Join(" ", arguments);
                string before = Information.SecondaryPrefix;
                Information.Config.SecondaryPrefix = prefix;
                await ReplyAsync($"Prefix changed from `{before}` to `{prefix}`!");
                Information.WriteOut();
                await ConsoleHelper.Log($"{Context.User} changed the prefix from {before} to {prefix}!", LogSeverity.Info);
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't change the prefix.. :confused:");
                await ConsoleHelper.Log($"Error chaning prefix, {ex.Message}!", LogSeverity.Error);
            }
        }
    }
}
