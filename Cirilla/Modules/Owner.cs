using Cirilla.Services.GuildConfig;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Owner : ModuleBase {
        [Command("log"), Summary("Upload the Bot's log")]
        public async Task GetLog() {
            try {
                IUser user = Context.User;
                if (!Helper.IsOwner(user)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                IDMChannel dm = await user.CreateDMChannelAsync();

                string file = Path.Combine(Information.Directory, "log_copy.txt");
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


        [Command("cmdlog"), Summary("Upload the Bot's command log")]
        public async Task GetCommandLog() {
            try {
                IUser user = Context.User;
                if (!Helper.IsOwner(user)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                IDMChannel dm = await user.CreateDMChannelAsync();
                await CommandLogger.Upload(dm);
                await ConsoleHelper.Log($"{Context.User} requested the bot log!", LogSeverity.Info);
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't send you the log.. :confused:");
                await ConsoleHelper.Log($"Error sending log, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("clearcmdlog"), Summary("Clear the Bot's command log")]
        public async Task ClearCommandLog() {
            try {
                IUser user = Context.User;
                if (!Helper.IsOwner(user)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                long kb = CommandLogger.Clear();
                await ReplyAsync($"Log cleared! ({kb} kB)");
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't clear the log.. :confused:");
                await ConsoleHelper.Log($"Error clearing log, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("game"), Summary("Change Bot's \"playing ..\" status")]
        public async Task SetGame([Summary("The new game")] [Remainder] string game) {
            try {
                IUser user = Context.User;
                if (!Helper.IsOwner(user)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                await Cirilla.Client.SetGameAsync(game);
                await ReplyAsync($"Now playing: _{game}_");
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't set the new game.. :confused:");
                await ConsoleHelper.Log($"Error setting game, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("announce"), Summary("Announce something in all guilds")]
        public async Task Announce([Summary("The text to send")] [Remainder] string text) {
            try {
                IUser user = Context.User;
                if (!Helper.IsOwner(user)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                int sent = 0;
                foreach (SocketGuild guild in Cirilla.Client.Guilds) {
                    try {
                        await guild.DefaultChannel.SendMessageAsync(text);
                        sent++;
                    } catch {
                        // could not send
                    }
                }
                await ReplyAsync($"Sent the announcement to {sent}/{Cirilla.Client.Guilds.Count} guilds!");
                await ConsoleHelper.Log(
                    $"{Context.User} announced \"{text}\" on {sent}/{Cirilla.Client.Guilds.Count} guilds!",
                    LogSeverity.Info);
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't announce that.. :confused:");
                await ConsoleHelper.Log($"Error announcing, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("togglescripts"), Summary("Enable or disable Roslyn Scripts")]
        public async Task ToggleScripts() {
            try {
                IUser user = Context.User;
                if (!Helper.IsOwner(user)) {
                    await ReplyAsync("Sorry, only the owner can reboot the bot!");
                    return;
                }

                GuildConfiguration config = GuildConfigManager.Get(Context.Guild.Id);
                config.EnableScripts = !config.EnableScripts;
                GuildConfigManager.Set(config);

                if (config.EnableScripts) {
                    await ReplyAsync("Scripts are now enabled for this guild! Use `$exec code`.");
                } else {
                    await ReplyAsync("Scripts are now disabled for this guild!");
                }
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't toggle scripts.. :confused:");
                await ConsoleHelper.Log($"Error changing script setting, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("reboot"), Summary("Reboot the Bot")]
        public async Task Reboot() {
            try {
                IUser user = Context.User;
                if (!Helper.IsOwner(user)) {
                    await ReplyAsync("Sorry, only the owner can reboot the bot!");
                    return;
                }

                await ReplyAsync("Rebooting.. :arrows_counterclockwise:");
                await ConsoleHelper.Log($"{Context.User} requested a bot reboot!", LogSeverity.Info);

                RebootBot();
            } catch (Exception ex) {
                //process not found? could not start/kill?
                await ReplyAsync("Whoops, unfortunately I couldn't reboot.. :confused:");
                await ConsoleHelper.Log($"Error rebooting, {ex.Message}!", LogSeverity.Error);
            }
        }

        private static async void RebootBot() {
            await Task.Delay(1000);
            await ConsoleHelper.Log("Handler done, stopping Bot..", LogSeverity.Debug);
            await Cirilla.Client.StopAsync();
            await Task.Delay(1000);
            Process current = Process.GetCurrentProcess();

            Process.Start(current.MainModule.FileName);

            await Task.Delay(1000);

            current.Kill();
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
                await ReplyAsync("Whoops, unfortunately I couldn't shutdown.. :confused:");
                await ConsoleHelper.Log($"Error shutting down, {ex.Message}!", LogSeverity.Error);
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
