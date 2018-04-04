using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Cirilla.Services.GuildConfig;
using Cirilla.Services.Pastebin;
using Discord;
using Discord.Commands;

namespace Cirilla.Modules
{
    public class Owner : ModuleBase
    {
        [Command("log")]
        [Summary("Upload the Bot's log")]
        public async Task GetLog()
        {
            try
            {
                var user = Context.User;
                if (!Helper.IsOwner(user))
                {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                IMessage message = await ReplyAsync("One sec..");

                string file = Path.Combine(Information.Directory, "log.txt");
                string log = File.ReadAllText(file);
                //upload to pastebin
                string link = await Pastebin.Post(log);

                var dm = await user.GetOrCreateDMChannelAsync();
                await dm.SendMessageAsync($"Here you go <{link}>");

                await message.DeleteAsync();

                ConsoleHelper.Log($"{Context.User} requested the bot log!", LogSeverity.Info);
            } catch (Exception ex)
            {
                await ReplyAsync(
                    "Whoops, unfortunately I couldn't send you the log.. I think it's a faulty API key :confused:");
                ConsoleHelper.Log($"Error sending log, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("clearlog")]
        [Summary("Clear the Bot's log")]
        public async Task ClearLog()
        {
            try
            {
                var user = Context.User;
                if (!Helper.IsOwner(user))
                {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                string file = Path.Combine(Information.Directory, "log.txt");
                long size = new FileInfo(file).Length;
                lock (Helper.Lock)
                {
                    File.WriteAllBytes(file, new byte[0]);
                }

                await ReplyAsync($"Log cleared! ({size / 1000} kB)");
                ConsoleHelper.Log($"{Context.User} cleared the bot log! ({size / 1000} kB)", LogSeverity.Info);
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't clear the log.. :confused:");
                ConsoleHelper.Log($"Error clear log, {ex.Message}!", LogSeverity.Error);
            }
        }


        [Command("cmdlog")]
        [Summary("Upload the Bot's command log")]
        public async Task GetCommandLog()
        {
            try
            {
                var user = Context.User;
                if (!Helper.IsOwner(user))
                {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                var dm = await user.GetOrCreateDMChannelAsync();
                await CommandLogger.Upload(dm, Context.Channel);
                ConsoleHelper.Log($"{Context.User} requested the bot log!", LogSeverity.Info);
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't send you the log.. :confused:");
                ConsoleHelper.Log($"Error sending log, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("clearcmdlog")]
        [Summary("Clear the Bot's command log")]
        public async Task ClearCommandLog()
        {
            try
            {
                var user = Context.User;
                if (!Helper.IsOwner(user))
                {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                long kb = CommandLogger.Clear();
                await ReplyAsync($"Log cleared! ({kb} kB)");
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't clear the log.. :confused:");
                ConsoleHelper.Log($"Error clearing log, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("game")]
        [Summary("Change Bot's \"playing ..\" status")]
        public async Task SetGame([Summary("The new game")] [Remainder] string game)
        {
            try
            {
                var user = Context.User;
                if (!Helper.IsOwner(user))
                {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                await Cirilla.Client.SetGameAsync(game);
                await ReplyAsync($"Now playing: _{game}_");
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't set the new game.. :confused:");
                ConsoleHelper.Log($"Error setting game, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("game")]
        [Summary("Reset Bot's \"playing ..\" status")]
        public async Task ResetGame()
        {
            try
            {
                var user = Context.User;
                if (!Helper.IsOwner(user))
                {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                await Cirilla.Client.SetGameAsync($"$help | {Cirilla.Client.Guilds.Count} Guilds");
                await ReplyAsync($"Now playing: _{Cirilla.Client.CurrentUser.Game?.Name}_");
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't reset the game.. :confused:");
                ConsoleHelper.Log($"Error resetting game, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("announce")]
        [Summary("Announce something in all guilds")]
        public async Task Announce([Summary("The text to send")] [Remainder]
            string text)
        {
            try
            {
                var user = Context.User;
                if (!Helper.IsOwner(user))
                {
                    await ReplyAsync($"Sorry, but only {Information.Owner} can announce!");
                    return;
                }

                int sent = 0;
                foreach (var guild in Cirilla.Client.Guilds)
                    try
                    {
                        await guild.DefaultChannel.SendMessageAsync(text);
                        sent++;
                    } catch
                    {
                        // could not send
                    }

                await ReplyAsync($"Sent the announcement to {sent}/{Cirilla.Client.Guilds.Count} guilds!");
                ConsoleHelper.Log(
                    $"{Context.User} announced \"{text}\" on {sent}/{Cirilla.Client.Guilds.Count} guilds!",
                    LogSeverity.Info);
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't announce that.. :confused:");
                ConsoleHelper.Log($"Error announcing, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("togglescripts")]
        [Summary("Enable or disable Roslyn Scripts")]
        public async Task ToggleScripts()
        {
            try
            {
                var user = Context.User;
                if (!Helper.IsOwner(user))
                {
                    await ReplyAsync($"Sorry, but only {Information.Owner} can toggle scripts!");
                    return;
                }

                var config = GuildConfigManager.Get(Context.Guild.Id);
                config.EnableScripts = !config.EnableScripts;
                GuildConfigManager.Set(config);

                if (config.EnableScripts) await ReplyAsync("Scripts are now enabled for this guild! Use `$exec code`.");
                else await ReplyAsync("Scripts are now disabled for this guild!");
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't toggle scripts.. :confused:");
                ConsoleHelper.Log($"Error changing script setting, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("reboot")]
        [Summary("Reboot the Bot")]
        public async Task Reboot()
        {
            try
            {
                var user = Context.User;
                if (!Helper.IsOwner(user))
                {
                    await ReplyAsync($"Sorry, but only {Information.Owner} can reboot the bot!");
                    return;
                }

                await ReplyAsync("Rebooting.. :arrows_counterclockwise:");
                ConsoleHelper.Log($"{Context.User} requested a bot reboot!", LogSeverity.Info);

                RebootBot();
            } catch (Exception ex)
            {
                //process not found? could not start/kill?
                await ReplyAsync("Whoops, unfortunately I couldn't reboot.. :confused:");
                ConsoleHelper.Log($"Error rebooting, {ex.Message}!", LogSeverity.Error);
            }
        }

        private static async void RebootBot()
        {
            await Task.Delay(1000);
            ConsoleHelper.Log("Handler done, stopping Bot..", LogSeverity.Debug);
            await Cirilla.Client.StopAsync();
            await Task.Delay(1000);
            var current = Process.GetCurrentProcess();

            Process.Start(current.MainModule.FileName);

            await Task.Delay(1000);

            current.Kill();
        }


        [Command("shutdown")]
        [Summary("Shutdown the Bot")]
        public async Task Shutdown()
        {
            try
            {
                var user = Context.User;
                if (!Helper.IsOwner(user))
                {
                    await ReplyAsync($"Sorry, but only {Information.Owner} can shutdown the bot!");
                    return;
                }

                await ReplyAsync("Shutting down.. :arrows_counterclockwise:");
                ConsoleHelper.Log($"{Context.User} requested a bot shutdown!", LogSeverity.Info);

                StopBot();
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't shutdown.. :confused:");
                ConsoleHelper.Log($"Error shutting down, {ex.Message}!", LogSeverity.Error);
            }
        }


        private static async void StopBot()
        {
            await Task.Delay(1000);
            ConsoleHelper.Log("Handler done, stopping Bot..", LogSeverity.Debug);
            await Cirilla.Client.StopAsync();
            await Task.Delay(1000);
            Process.GetCurrentProcess().Kill();
        }
    }
}