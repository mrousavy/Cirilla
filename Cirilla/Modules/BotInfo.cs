using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class BotInfo : ModuleBase {
        [Command("info"), Summary("Shows host information")]
        public async Task Info() {
            try {
                string mname = Environment.MachineName;
                string pre =
                    $"{Information.Prefix}, {Information.SecondaryPrefix}, {Context.Client.CurrentUser.Mention}";
                int cores = Environment.ProcessorCount;
                Process current = Process.GetCurrentProcess();

                EmbedBuilder builder = new EmbedBuilder {
                    Author = new EmbedAuthorBuilder {
                        Name = "Bot Information",
                        IconUrl = Information.IconUrl
                    },
                    Color = new Color(50, 125, 0)
                };
                builder.AddInlineField("Uptime", GetUptime());
                builder.AddInlineField("Machine", mname);
                builder.AddInlineField("Core #", $"{cores} cores");
                builder.AddInlineField("Max RAM usage", $"{((double)current.PeakWorkingSet64 / 1024 / 1024):#.#} MB");
                builder.AddInlineField("Prefixes", pre);
                builder.AddInlineField("Source Code", $"[GitHub]({Information.RepoUrl})");
                builder.AddInlineField("My Senpai", Information.Owner);

                await ReplyAsync("", embed: builder.Build());
                await ConsoleHelper.Log($"{Context.User} requested Bot/Host Information!", LogSeverity.Info);
            } catch (Exception ex) {
                await ReplyAsync("Sorry, I can't send you that information right now!");
                await ConsoleHelper.Log($"Error retrieving Host information {ex.Message}", LogSeverity.Error);
            }
        }

        [Command("invite"), Summary("Show Guild join link for bot")]
        public async Task Invite() {
            await ReplyAsync(Information.InviteLink);
        }

        [Command("uptime"), Summary("Shows bot uptime")]
        public async Task Uptime() {
            try {
                TimeSpan tspan = (DateTime.Now - Program.StartTime);
                if (tspan.TotalHours >= 1) {
                    await ReplyAsync(
                        "I'm already running for " + tspan.ToString("m'm 's's'") + ", I'm tired :confused:");
                } else if (tspan.TotalMinutes >= 1) {
                    await ReplyAsync("I'm running for " + tspan.ToString("m'm 's's'"));
                } else {
                    await ReplyAsync("I'm running for " + tspan.ToString("s's'"));
                }
                await ConsoleHelper.Log($"{Context.User} requested Uptime!", LogSeverity.Info);
            } catch (Exception ex) {
                await ReplyAsync("Sorry, I can't send you that information right now!");
                await ConsoleHelper.Log($"Error retrieving uptime information {ex.Message}", LogSeverity.Error);
            }
        }

        private static string GetUptime() {
            TimeSpan tspan = (DateTime.Now - Program.StartTime);
            string uptime;
            if (tspan.TotalHours >= 1) {
                uptime = tspan.ToString("h'h 'm'm 's's'");
            } else if (tspan.TotalMinutes >= 1) {
                uptime = tspan.ToString("m'm 's's'");
            } else {
                uptime = tspan.ToString("s's'");
            }
            return uptime;
        }


        [Command("run"), Summary("Shows how to run this bot")]
        public async Task Run() {
            try {
                EmbedBuilder builder = new EmbedBuilder {
                    Title = "How to run Cirilla Bot:",
                    Color = new Color(50, 125, 0)
                };
                builder.AddField("1. Clone", $"Clone project from GitHub (git bash: `git clone {Information.RepoUrl}`");
                builder.AddField("2. Open Bash/CLI",
                    "Run bash or any other command line tool and navigate into the Project folder: " +
                    "`cd C:\\Some\\Directory\\Cirilla\\Cirilla`");
                builder.AddField("4. DNX Restore", "Run `dotnet restore` in the Cirilla Project Folder");
                builder.AddField("5. DNX Run",
                    "Run `dotnet run` (Requires [.NET Core Tools](https://www.microsoft.com/net/download/core#/runtime))");

                await ReplyAsync("", embed: builder.Build());
            } catch {
                // channel not available anymore? any unexpected error
            }
        }

        [Command("source"), Summary("Shows source code of Cirilla")]
        public async Task Source() {
            await ReplyAsync($"I'm written in C#.NET Core, my source code is on GitHub; {Information.RepoUrl}");
        }
    }
}