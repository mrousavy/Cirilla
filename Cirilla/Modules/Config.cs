using Discord;
using Discord.Commands;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Config : ModuleBase {
        [Command("config"), Summary("Show bot config")]
        public async Task ShowConfig() {
            EmbedBuilder builder = new EmbedBuilder {
                Color = new Color(50, 125, 0),
                Author = new EmbedAuthorBuilder {
                    IconUrl = Information.IconUrl,
                    Name = "Cirilla Config"
                }
            };
            builder.AddInlineField("Default Text Channel", Information.TextChannel);
            builder.AddInlineField("Prefixes",
                $"{Information.Prefix}, {Information.SecondaryPrefix}, {Context.Client.CurrentUser.Mention}");
            builder.AddInlineField("Repo URL", $"[GitHub]({Information.RepoUrl})");
            builder.AddInlineField("Senpai", Information.Owner);
            builder.AddInlineField("XP Drop Interval", $"Every {Information.XpGiveInterval / 1000} Seconds");
            builder.AddInlineField("Regain gift XP", $"{100 / Information.OwnXp}% XP");
            builder.AddInlineField("XP Level Factor", $"Lvl = PreviousLvl * {Information.XpFactor}");
            builder.AddInlineField("Random 200XP Chance", $"1 in {Information.GiveRandomXpChance} chance");
            builder.AddInlineField("Random Reaction Chance", $"1 in {Information.RandomReactionChance} chance");
            builder.AddInlineField("Votekick enabled", Information.AllowVotekick);
            builder.AddInlineField("Votekick Emojis", $"{Information.VotekickYes} {Information.VotekickNo}");
            builder.AddInlineField("Votekick expiration", $"{Information.VotekickExpire / 1000} Seconds");
            builder.AddInlineField("News Interval", $"Every {Information.NewsInterval} Hours");
            builder.AddInlineField("Allow Scripts", Information.AllowScripts);
            builder.AddInlineField("Scripts Timeout",
                $"C: {Information.CompileTimeout / 1000}s | X: {Information.ExecutionTimeout / 1000}s");

            await ReplyAsync("", embed: builder.Build());
        }

        [Command("reload"), Summary("Reload bot configuration")]
        public async Task ReloadConfig() {
            if (Context.User is IGuildUser user) {
                if (user.GuildPermissions.Administrator) {
                    Information.LoadInfo();
                    await ReplyAsync($"Config reloaded! Type `{Information.Prefix}config` to see the config");
                } else {
                    await ReplyAsync("You're not allowed to reload my config!");
                }
            }
        }




        #region Set
        //[Command("config"), Summary("Set a specific setting (string) in bot config")]
        public async Task SetConfig([Summary("The Property you want to set")] string property, [Summary("The value for the specific property")] string value) {
            await SetProperty(property, value);
        }
        //[Command("config"), Summary("Set a specific setting (bool) in bot config")]
        public async Task SetConfig([Summary("The Property you want to set")] string property, [Summary("The value for the specific property")] bool value) {
            await SetProperty(property, value);
        }
        //[Command("config"), Summary("Set a specific setting (int) in bot config")]
        public async Task SetConfig([Summary("The Property you want to set")] string property, [Summary("The value for the specific property")] int value) {
            await SetProperty(property, value);
        }
        //[Command("config"), Summary("Set a specific setting (double) in bot config")]
        public async Task SetConfig([Summary("The Property you want to set")] string property, [Summary("The value for the specific property")] double value) {
            await SetProperty(property, value);
        }
        //[Command("config"), Summary("Set a specific setting (LogSeverity) in bot config")]
        public async Task SetConfig([Summary("The Property you want to set")] string property, [Summary("The value for the specific property")] LogSeverity value) {
            await SetProperty(property, value);
        }


        public async Task SetProperty(string property, object value) {
            try {
                PropertyInfo pinfo = typeof(Configuration).GetProperty(property);

                if (pinfo.PropertyType == value.GetType()) {
                    pinfo.SetValue(Information.Config, value);
                    Information.WriteOut();
                    await ReplyAsync($"{property} was set to {value}! Use `{Information.Prefix}config` to see the current config!");
                } else {
                    await ReplyAsync($"{property} does not accept values of Type {value.GetType()}");
                }
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Error setting Information.Config.{property}! ({ex.Message})", LogSeverity.Error);
            }
        }
        #endregion
    }
}