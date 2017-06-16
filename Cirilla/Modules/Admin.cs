using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Admin : ModuleBase {
        [Command("prefix"), Summary("Change prefix")]
        public async Task ChangePrefix([Summary("New prefix")] [Remainder] string prefix) {
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
                Information.WriteOut();
                await ConsoleHelper.Log($"{Context.User} changed the prefix from {before} to {prefix}!",
                    LogSeverity.Info);
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't change the prefix.. :confused:");
                await ConsoleHelper.Log($"Error chaning prefix, {ex.Message}!", LogSeverity.Error);
            }
        }


        [Command("shutdown"), Summary("Shutdown the Bot")]
        public async Task Shutdown() {
            try {
                IGuildUser user = Context.User as IGuildUser;
                if (user == null) {
                    return;
                }
                if (!user.GuildPermissions.Has(GuildPermission.ManageMessages)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                await ReplyAsync("Shutting down.. :arrows_counterclockwise:");
                await ConsoleHelper.Log($"{Context.User} requested a bot shutdown!", LogSeverity.Info);

                StopBot();
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't reboot.. :confused:");
                await ConsoleHelper.Log($"Error rebooting, {ex.Message}!", LogSeverity.Error);
            }
        }


        private static async void StopBot() {
            await Task.Delay(1000);
            ConsoleHelper.Log("Handler done, stopping Bot..", LogSeverity.Debug).Wait();
            await Cirilla.Client.StopAsync();
            await Task.Delay(1000);
            Process.GetCurrentProcess().Kill();
        }
    }
}