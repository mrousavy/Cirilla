﻿using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class UserInfo : ModuleBase {
        [Command("stats"), Summary("Shows statistics for a specific user")]
        public async Task Stats(IGuildUser user) {
            try {
                string uname = user.Username ?? "?";
                string nick = user.Nickname ?? "/";
                string createdAt = user.CreatedAt.ToString("d") ?? "/";

                DateTimeOffset? joinedAtDate = user.JoinedAt;
                string joinedAt = joinedAtDate != null ? joinedAtDate.Value.ToString("d") : "/";
                string status = user.Status.ToString() ?? "/";
                Game? nullableGame = user.Game;
                string game = nullableGame != null ? nullableGame.Value.ToString() : "/";
                string xp = XpManager.Get(user).Xp.ToString();

                EmbedBuilder builder = new EmbedBuilder {
                    Author = new EmbedAuthorBuilder {
                        Name = user.IsBot ? $"{uname} (Bot)" : $"{uname}",
                        IconUrl = user.GetAvatarUrl()
                    },
                    Color = new Color(50, 125, 0)
                };

                builder.AddField("Nickname", nick);
                builder.AddField("Created At", createdAt);
                builder.AddField("Joined At", joinedAt);
                builder.AddField("Status", status);
                builder.AddField("Game", game);
                builder.AddField("XP", xp);

                await ReplyAsync("", embed: builder.Build());
            } catch (Exception ex) {
                await ReplyAsync($"Error! ({ex.Message})");
            }
        }


        [Command("stats"), Summary("Shows statistics for user")]
        public async Task Stats() {
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
                    string xp = XpManager.Get(user).Xp.ToString();

                    EmbedBuilder builder = new EmbedBuilder {
                        Author = new EmbedAuthorBuilder {
                            Name = user.IsBot ? $"{uname} (Bot)" : $"{uname}",
                            IconUrl = user.GetAvatarUrl()
                        },
                        Color = new Color(50, 125, 0)
                    };
                    builder.AddField("Nickname", nick);
                    builder.AddField("Created At", createdAt);
                    builder.AddField("Joined At", joinedAt);
                    builder.AddField("Status", status);
                    builder.AddField("Game", game);
                    builder.AddField("XP", xp);

                    await ReplyAsync("", embed: builder.Build());
                }
            } catch (Exception ex) {
                await ReplyAsync($"Error! ({ex.Message})");
            }
        }
    }
}
