using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class BotInfo : ModuleBase {
        [Command("info"), Summary("Shows host information")]
        public async Task Info() {
            try {
                string uname = Information.Username;
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
                builder.AddInlineField("Username", uname);
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
    }
}
