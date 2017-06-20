using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Owner : ModuleBase {
        [Command("prefix"), Summary("Change prefix")]
        public async Task ChangePrefix([Summary("New prefix")] [Remainder] string prefix) {
            try {
                IUser user = Context.User;
                if (!Helper.IsOwner(user)) {
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

        [Command("log"), Summary("Upload the Bot's log")]
        public async Task GetLog() {
            try {
                IUser user = Context.User;
                if (!Helper.IsOwner(user)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                IDMChannel dm = await user.CreateDMChannelAsync();

                string file = Path.Combine(Information.Directory, "botlog.txt");
                lock (Helper.Lock)
                    File.Copy(Path.Combine(Information.Directory, "log.txt"), file);
                await dm.SendFileAsync(file, "Here you go");
                lock (Helper.Lock)
                    File.Delete(file);

                await ConsoleHelper.Log($"{Context.User} requested the bot log!", LogSeverity.Info);
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't send you the log.. :confused:");
                await ConsoleHelper.Log($"Error sending log, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("clearlog"), Summary("Clear the Bot's log")]
        public async Task ClearLog() {
            try {
                IUser user = Context.User;
                if (!Helper.IsOwner(user)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                string file = Path.Combine(Information.Directory, "log.txt");
                long size = new FileInfo(file).Length;
                lock (Helper.Lock)
                    File.WriteAllBytes(file, new byte[0]);
                await ReplyAsync($"Log cleared! ({size / 1000} kB)");
                await ConsoleHelper.Log($"{Context.User} cleared the bot log! ({size / 1000} kB)", LogSeverity.Info);
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't clear the log.. :confused:");
                await ConsoleHelper.Log($"Error clear log, {ex.Message}!", LogSeverity.Error);
            }
        }
        [Command("game"), Summary("Change Bot's \"playing ..\" status")]
        public async Task SetGame([Summary("The new game")] string game) {
            try {
                IUser user = Context.User;
                if (!Helper.IsOwner(user)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                await Cirilla.Client.SetGameAsync(game);
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't set the new game.. :confused:");
                await ConsoleHelper.Log($"Error setting game, {ex.Message}!", LogSeverity.Error);
            }
        }



        [Command("shutdown"), Summary("Shutdown the Bot")]
        public async Task Shutdown() {
            try {
                IUser user = Context.User;
                if (!Helper.IsOwner(user)) {
                    await ReplyAsync("Sorry, only the owner can shutdown the bot!");
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
