using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class BotInfo : ModuleBase {
        [Command("info"), Summary("Shows host information")]
        public async Task Info() {
            try {
                TimeSpan tspan = (DateTime.Now - Program.StartTime);
                string uptime = "";
                if (tspan.TotalHours >= 1) {
                    uptime = tspan.ToString("h'h 'm'm 's's'");
                } else if (tspan.TotalMinutes >= 1) {
                    uptime = tspan.ToString("m'm 's's'");
                } else {
                    uptime = tspan.ToString("s's'");
                }
                string mname = Environment.MachineName;
                string nl = Environment.NewLine;
                char pre = Information.Prefix;
                int cores = Environment.ProcessorCount;

                EmbedBuilder builder = new EmbedBuilder {
                    Author = new EmbedAuthorBuilder {
                        Name = "Bot Information",
                        IconUrl = Information.IconUrl
                    },
                    Color = new Color(50, 125, 0)
                };
                builder.AddInlineField("Uptime", uptime);
                builder.AddInlineField("Machine", mname);
                builder.AddInlineField("Core #", cores + " cores");
                builder.AddInlineField("Prefix", pre);
                builder.AddInlineField("Source Code", $"[GitHub]({Information.RepoUrl})");
                builder.AddInlineField("My Senpai", Information.Senpai);
                //builder.AddField("Username", uname);
                //builder.AddField("Machine", mname);
                //builder.AddField("Core #", cores + " cores");
                //builder.AddField("Prefix", pre);
                //builder.AddField("Source Code", $"[GitHub]({Information.RepoUrl})");

                await ReplyAsync("", embed: builder.Build());
            } catch (Exception ex) {
                await ReplyAsync($"Error! ({ex.Message})");
            }
        }


        [Command("run"), Summary("Shows how to run bot")]
        public async Task Run() {
            try {
                EmbedBuilder builder = new EmbedBuilder {
                    Title = "How to run Cirilla Bot:",
                    Color = new Color(50, 125, 0)
                };
                builder.AddField("1. Clone", $"Clone project from GitHub (git bash: `git clone {Information.RepoUrl}`");
                builder.AddField("2. Open Bash/CLI", "Run bash or any other command line tool and navigate into the Project folder: " +
                    "`cd C:\\Some\\Directory\\Cirilla\\Cirilla`");
                builder.AddField("4. DNX Restore", "Run `dotnet restore` in the Cirilla Project Folder");
                builder.AddField("5. DNX Run", "Run `dotnet run` (Requires [.NET Core Tools](https://www.microsoft.com/net/download/core#/runtime)");

                await ReplyAsync("", embed: builder.Build());
            } catch (Exception ex) {
                await ReplyAsync($"Error! ({ex.Message})");
            }
        }
    }
}
