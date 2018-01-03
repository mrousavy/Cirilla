using System.Threading.Tasks;
using Cirilla.Services.GuildConfig;
using Discord;
using Discord.Commands;

namespace Cirilla.Modules {
    public class Config : ModuleBase {
        [Command("config")]
        [Summary("Show bot config")]
        public async Task ShowConfig() {
            var config = GuildConfigManager.Get(Context.Guild.Id);

            string prefixes = string.Empty;
            if (config.EnablePrimaryPrefix) prefixes += $"{Information.Prefix}, ";
            prefixes += $"{config.Prefix}, {Context.Client.CurrentUser.Mention}";

            var builder = new EmbedBuilder {
                Color = new Color(50, 125, 0),
                Author = new EmbedAuthorBuilder {
                    IconUrl = Information.IconUrl,
                    Name = "Cirilla Config"
                }
            };
            builder.AddInlineField("Default Text Channel", Information.TextChannel);
            builder.AddInlineField("Prefixes", prefixes);
            builder.AddInlineField("Repo URL", $"[GitHub]({Information.RepoUrl})");
            builder.AddInlineField("Senpai", Information.Owner);
            builder.AddInlineField("Enable XP System", config.EnableXpSystem);
            builder.AddInlineField("XP Drop Interval", $"Every {Information.XpGiveInterval / 1000} Seconds");
            builder.AddInlineField("Regain gift XP", $"{100 / Information.OwnXp}% XP");
            builder.AddInlineField("XP Level Factor", $"Lvl = PreviousLvl * {Information.XpFactor}");
            builder.AddInlineField("Random 200XP Chance", $"1 in {Information.GiveRandomXpChance} chance");
            builder.AddInlineField("Random Reaction Chance", $"1 in {Information.RandomReactionChance} chance");
            builder.AddInlineField("Votekick enabled", Information.AllowVotekick);
            builder.AddInlineField("Votekick Emojis", $"{Information.VotekickYes} {Information.VotekickNo}");
            builder.AddInlineField("Votekick expiration", $"{Information.VotekickExpire / 1000} Seconds");
            builder.AddInlineField("News Interval", $"Every {Information.NewsInterval} Hours");
            builder.AddInlineField("Allow Scripts", config.EnableScripts);
            builder.AddInlineField("Scripts Timeout",
                $"C: {Information.CompileTimeout / 1000}s | X: {Information.ExecutionTimeout / 1000}s");

            await ReplyAsync("", embed: builder.Build());
        }

        [Command("reload")]
        [Summary("Reload bot configuration")]
        public async Task ReloadConfig() {
            if (Context.User is IGuildUser user)
                if (Helper.IsOwner(user)) {
                    Information.LoadInfo();
                    await ReplyAsync($"Config reloaded! Type `{Information.Prefix}config` to see the config");
                } else {
                    await ReplyAsync("You're not allowed to reload my config!");
                }
        }
    }
}