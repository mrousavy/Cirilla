using Cirilla.Services.Xp;
using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class UserInfo : ModuleBase {
        [Command("info"), Summary("Shows Information for a specific user")]
        public async Task Info(IGuildUser user) {
            try {
                string uname = user.Username ?? "?";
                string nick = user.Nickname ?? "/";
                string createdAt = user.CreatedAt.ToString("d") ?? "/";

                DateTimeOffset? joinedAtDate = user.JoinedAt;
                string joinedAt = joinedAtDate != null ? joinedAtDate.Value.ToString("d") : "/";
                string status = user.Status.ToString() ?? "/";
                Game? nullableGame = user.Game;
                string game = nullableGame != null ? nullableGame.Value.ToString() : "/";
                string xp = XpManager.Get(Context.Guild, user).Xp.ToString();

                EmbedBuilder builder = new EmbedBuilder {
                    Author = new EmbedAuthorBuilder {
                        Name = user.IsBot ? $"{uname} (Bot)" : $"{uname}",
                        IconUrl = user.GetAvatarUrl()
                    },
                    Color = new Color(50, 125, 0)
                };

                builder.AddInlineField("Nickname", nick);
                builder.AddInlineField("Created At", createdAt);
                builder.AddInlineField("Joined At", joinedAt);
                builder.AddInlineField("Status", status);
                builder.AddInlineField("Game", game);
                builder.AddInlineField("XP", xp);

                await ReplyAsync("", embed: builder.Build());
            } catch (Exception ex) {
                await ReplyAsync($"Error! ({ex.Message})");
            }
        }


        [Command("info"), Summary("Shows information for user")]
        public async Task Info() {
            try {
                if (Context.User is IGuildUser user) {
                    string uname = user.Username ?? "?";
                    string nick = user.Nickname ?? "/";
                    string createdAt = user.CreatedAt.ToString("d") ?? "/";

                    DateTimeOffset? joinedAtDate = user.JoinedAt;
                    string joinedAt = joinedAtDate != null ? joinedAtDate.Value.ToString("d") : "/";
                    string status = user.Status.ToString() ?? "/";
                    Game? nullableGame = user.Game;
                    string game = nullableGame != null ? nullableGame.Value.ToString() : "/";
                    string xp = XpManager.Get(Context.Guild, user).Xp.ToString();

                    EmbedBuilder builder = new EmbedBuilder {
                        Author = new EmbedAuthorBuilder {
                            Name = user.IsBot ? $"{uname} (Bot)" : $"{uname}",
                            IconUrl = user.GetAvatarUrl()
                        },
                        Color = new Color(50, 125, 0)
                    };
                    builder.AddInlineField("Nickname", nick);
                    builder.AddInlineField("Created At", createdAt);
                    builder.AddInlineField("Joined At", joinedAt);
                    builder.AddInlineField("Status", status);
                    builder.AddInlineField("Game", game);
                    builder.AddInlineField("XP", xp);

                    await ReplyAsync("", embed: builder.Build());
                }
            } catch (Exception ex) {
                await ReplyAsync($"Error! ({ex.Message})");
            }
        }
    }
}