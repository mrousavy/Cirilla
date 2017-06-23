using Cirilla.Services.GuildConfig;
using Cirilla.Services.Xp;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Xp : ModuleBase {
        [Command("xp"), Summary("Give a user XP")]
        public async Task Give([Summary("The user to give XP")] IGuildUser user,
            [Summary("The amount of XP you want to give the user")] int xp) {
            try {
                if (!GuildConfigManager.Get(Context.Guild.Id).EnableXpSystem) {
                    await ReplyAsync("XP System is disabled for this Guild! An Admin can use `$togglexp`!");
                    return;
                }

                if (user.Id == Context.User.Id) {
                    await ReplyAsync("Nice try, you can't give yourself XP!");
                    return;
                }
                if (xp == 0) {
                    await ReplyAsync("Wow man. How generous. :+1:");
                    return;
                }
                bool canDrain = false; //Is sender Admin?
                bool drainOwn = true; //False if Admin is draining XP
                if (Context.User is IGuildUser guilduser && guilduser.GuildPermissions.Administrator) {
                    canDrain = true;
                }
                if (xp < 1) {
                    if (!canDrain) {
                        await ReplyAsync("Nice try, you can't drain XP from Users!");
                        return;
                    } else {
                        drainOwn = false;
                    }
                }


                int ownXp = xp / Information.OwnXp;

                UserXp userxp = XpManager.Get(Context.Guild, Context.User);

                if (userxp.Xp >= xp) {
                    int lvlBefore = XpManager.Get(Context.Guild, user).Level;
                    XpManager.Update(Context.Guild, user, xp);
                    int lvlAfter = XpManager.Get(Context.Guild, user).Level;

                    //Drain XP from Sender, minus own XP (you get 1/100 from donations)
                    if (drainOwn) {
                        XpManager.Update(Context.Guild, Context.User, -xp + ownXp);
                    }

                    if (xp < 1) {
                        await ReplyAsync(
                            $"{Helper.GetName(Context.User)} drained {Helper.GetName(user)} {-xp} {GetNameForXp()}! :money_with_wings:");
                    } else {
                        await ReplyAsync(
                            $"{Helper.GetName(Context.User)} gave {Helper.GetName(user)} {xp} {GetNameForXp()}! :money_with_wings:");
                    }

                    if (lvlAfter > lvlBefore) {
                        await ReplyAsync($":tada: {Helper.GetName(user)} was promoted to level {lvlAfter}! :tada:");
                    }
                    await ConsoleHelper.Log($"{Helper.GetName(Context.User)} gave {Helper.GetName(user)} {xp}XP!",
                        LogSeverity.Info);
                } else {
                    await ReplyAsync(
                        $"You can't give {xp} XP to {Helper.GetName(user)}, you only have {userxp.Xp} XP!");
                }
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not give XP to {Helper.GetName(user)}! ({ex.Message})",
                    LogSeverity.Error);
                await ReplyAsync("Whoops, I couldn't give XP to that user!");
            }
        }


        [Command("setxp"), Summary("Set a user's XP (Admin only)")]
        public async Task Set([Summary("The user to set XP")] IGuildUser user,
            [Summary("The amount of XP you want to set")] int xp) {
            try {
                if (Context.User is IGuildUser setter) {
                    if (!GuildConfigManager.Get(Context.Guild.Id).EnableXpSystem) {
                        await ReplyAsync("XP System is disabled for this Guild! An Admin can use `$togglexp`!");
                        return;
                    }

                    if (!Helper.IsOwner(setter) && !setter.GuildPermissions.ManageMessages) {
                        await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                        return;
                    }

                    UserXp userxp = XpManager.Get(Context.Guild, user);
                    XpManager.Update(Context.Guild, user, xp - userxp.Xp);
                    await ReplyAsync($"{Helper.GetName(user)} set {user.Mention}'s XP to {xp}!");
                } else {
                    await ReplyAsync("You can't use that command in DM!");
                }
                await ConsoleHelper.Log($"{Helper.GetName(Context.User)} set {Helper.GetName(user)}'s XP to {xp}!",
                    LogSeverity.Info);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not set {Helper.GetName(user)}'s XP! ({ex.Message})",
                    LogSeverity.Error);
                await ReplyAsync("Whoops, I couldn't give XP to that user!");
            }
        }

        [Command("xp"), Summary("Show own XP")]
        public async Task Info() {
            try {
                if (!GuildConfigManager.Get(Context.Guild.Id).EnableXpSystem) {
                    await ReplyAsync("XP System is disabled for this Guild! An Admin can use `$togglexp`!");
                    return;
                }
                UserXp xp = XpManager.Get(Context.Guild, Context.User);

                EmbedBuilder builder = new EmbedBuilder {
                    Author = new EmbedAuthorBuilder {
                        Name = Context.User.IsBot
                            ? $"{Helper.GetName(Context.User)}'s XP (Bot)"
                            : $"{Helper.GetName(Context.User)}'s XP",
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Color = new Color(50, 125, 0)
                };
                builder.AddInlineField("XP", xp.Xp);
                builder.AddInlineField("Level", xp.Level);
                builder.AddInlineField("Next Level", XpManager.GetXp(xp.Level + 1) - xp.Xp);

                await ReplyAsync("", embed: builder.Build());
            } catch {
                await ReplyAsync("I couldn't look up any Info about you, sorry..");
            }
        }

        [Command("xp"), Summary("Show XP of given User")]
        public async Task Info([Summary("The user you want to display XP")] IGuildUser user) {
            try {
                if (!GuildConfigManager.Get(Context.Guild.Id).EnableXpSystem) {
                    await ReplyAsync("XP System is disabled for this Guild! An Admin can use `$togglexp`!");
                    return;
                }

                UserXp xp = XpManager.Get(Context.Guild, user);

                EmbedBuilder builder = new EmbedBuilder {
                    Author = new EmbedAuthorBuilder {
                        Name = user.IsBot ? $"{Helper.GetName(user)}'s XP (Bot)" : $"{Helper.GetName(user)}'s XP",
                        IconUrl = user.GetAvatarUrl()
                    },
                    Color = new Color(50, 125, 0)
                };
                builder.AddInlineField("XP", xp.Xp);
                builder.AddInlineField("Level", xp.Level);
                builder.AddInlineField("Next Level", XpManager.GetXp(xp.Level + 1) - xp.Xp);

                await ReplyAsync("", embed: builder.Build());
            } catch {
                await ReplyAsync($"I couldn't look up any Info about {Helper.GetName(user)}, sorry..");
            }
        }


        [Command("stats"), Summary("Show XP Statistics/Ranking for Guild")]
        public async Task Stats() {
            try {
                if (!GuildConfigManager.Get(Context.Guild.Id).EnableXpSystem) {
                    await ReplyAsync("XP System is disabled for this Guild! An Admin can use `$togglexp`!");
                    return;
                }

                List<UserXp> xps = XpManager.Top10(Context.Guild);

                if (xps.Count < 1) {
                    await ReplyAsync("I couldn't find any users..");
                    return;
                }

                EmbedBuilder builder = new EmbedBuilder {
                    Author = new EmbedAuthorBuilder {
                        Name = $"XP Ranking for {Context.Guild.Name} 🏆"
                    },
                    ThumbnailUrl = Context.Guild.IconUrl,
                    Color = new Color(255, 163, 50),
                    Footer = new EmbedFooterBuilder {
                        Text = $"..out of {XpManager.Get(Context.Guild).Count} total XP users"
                    }
                };

                int max = xps.Count > 10 ? 10 : xps.Count;
                for (int i = 0; i < max; i++) {
                    UserXp xp = xps[i];
                    IUser user = await Context.Guild.GetUserAsync(xp.UserId);
                    string place;
                    switch (i) {
                        case 0:
                            place = ":first_place: 1";
                            break;
                        case 1:
                            place = ":second_place: 2";
                            break;
                        case 2:
                            place = ":third_place: 3";
                            break;
                        default:
                            place = $"{i + 1}";
                            break;
                    }
                    builder.AddField($"{place}. {Helper.GetName(user)}", $"XP: **{xp.Xp}** | Level: **{xp.Level}**");
                }

                try {
                    UserXp firstXp = xps[0];
                    IUser firstUser = await Context.Guild.GetUserAsync(firstXp.UserId);
                    await ReplyAsync($"Good job **{Helper.GetName(firstUser)}** on being first place with **{firstXp.Xp}** XP!", embed: builder.Build());
                } catch {
                    await ReplyAsync("", embed: builder.Build());
                }
            } catch {
                await ReplyAsync("I couldn't do a full ranking for that guild, sorry.. :confused:");
            }
        }


        public static string GetNameForXp() {
            //7 times XP (so "XP" is more common) and 4 times other units
            string[] names = { "XP", "XP", "XP", "XP", "XP", "XP", "XP", "Robux", "Euros", "Schilling", "Bitcoins" };
            return names[Program.Random.Next(0, names.Length + 1)];
        }
    }
}
