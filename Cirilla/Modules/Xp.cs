using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Xp : ModuleBase {
        [Command("xp"), Summary("Give a user XP")]
        public async Task Give([Summary("The user to give XP")]IGuildUser user, [Summary("The amount of XP you want to give the user")]int xp) {
            try {
                if (user.Id == Context.User.Id) {
                    await ReplyAsync("Nice try, you can't give yourself XP!");
                    return;
                }
                if (xp == 0) {
                    await ReplyAsync("Wow man. How generous. :+1:");
                    return;
                }
                if (Context.User is IGuildUser guilduser && (xp < 1 && !guilduser.GuildPermissions.Administrator)) {
                    await ReplyAsync("Nice try, you can't drain XP from Users!");
                    return;
                }
                int ownXp = xp / Information.OwnXp;

                XpManager.UserXp userxp = XpManager.Get(Context.User);

                if (userxp.XpReserve >= xp) {
                    int lvlBefore = XpManager.Get(user).Level;
                    XpManager.Update(user, xp, 0);
                    int lvlAfter = XpManager.Get(user).Level;

                    //Drain XP from Sender, minus own XP (you get 1/100 from donations)
                    XpManager.Update(Context.User, ownXp, -xp);

                    await ReplyAsync($"{Helper.GetName(Context.User)} gave {Helper.GetName(user)} {xp} {GetNameForXp()}! :money_with_wings:");

                    if (lvlAfter > lvlBefore) {
                        await ReplyAsync($":tada: {Helper.GetName(user)} was promoted to level {lvlAfter}! :tada:");
                    }
                } else {
                    await ReplyAsync($"You can't give {xp} XP to {Helper.GetName(user)}, you only have {userxp.XpReserve} XP!");
                }
                await ConsoleHelper.Log($"{Helper.GetName(Context.User)} gave {Helper.GetName(user)} {xp}XP!", LogSeverity.Info);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not give XP to {Helper.GetName(user)}! ({ex.Message})", LogSeverity.Error);
                await ReplyAsync("Whoops, I couldn't give XP to that user!");
            }
        }


        [Command("setxp"), Summary("Set a user's XP (Admin only)")]
        public async Task Set([Summary("The user to set XP")]IGuildUser user, [Summary("The amount of XP you want to set")]int xp) {
            try {
                if (Context.User is IGuildUser setter) {
                    if (!setter.GuildPermissions.Administrator) {
                        await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                        return;
                    }

                    XpManager.UserXp userxp = XpManager.Get(user);
                    XpManager.Update(user, xp - userxp.Xp, 0);
                    await ReplyAsync($"{Helper.GetName(user)} set {user.Mention}'s XP to {xp}!");
                } else {
                    await ReplyAsync("You can't use that command in DM!");
                }
                await ConsoleHelper.Log($"{Helper.GetName(Context.User)} set {Helper.GetName(user)}'s XP to {xp}!", LogSeverity.Info);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not set {Helper.GetName(user)}'s XP! ({ex.Message})", LogSeverity.Error);
                await ReplyAsync("Whoops, I couldn't give XP to that user!");
            }
        }

        [Command("xp"), Summary("Show own XP")]
        public async Task Info() {
            try {
                XpManager.UserXp xp = XpManager.Get(Context.User);

                EmbedBuilder builder = new EmbedBuilder {
                    Author = new EmbedAuthorBuilder {
                        Name = Context.User.IsBot ? $"{Helper.GetName(Context.User)}'s XP (Bot)" : $"{Helper.GetName(Context.User)}'s XP",
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Color = new Color(50, 125, 0)
                };
                builder.AddField("XP", xp.Xp);
                builder.AddField("Reserve XP", xp.XpReserve);
                builder.AddField("Level", xp.Level);
                builder.AddField("Next Level", XpManager.GetXp(xp.Level + 1) - xp.Xp);

                await ReplyAsync("", embed: builder.Build());
            } catch {
                await ReplyAsync("I couldn't look up any Info about you, sorry..");
            }
        }

        [Command("xp"), Summary("Show XP of given User")]
        public async Task Info([Summary("The user you want to display XP")]IGuildUser user) {
            try {
                XpManager.UserXp xp = XpManager.Get(user);

                EmbedBuilder builder = new EmbedBuilder {
                    Author = new EmbedAuthorBuilder {
                        Name = user.IsBot ? $"{Helper.GetName(user)}'s XP (Bot)" : $"{Helper.GetName(user)}'s XP",
                        IconUrl = user.GetAvatarUrl()
                    },
                    Color = new Color(50, 125, 0)
                };
                builder.AddField("XP", xp.Xp);
                builder.AddField("Reserve XP", xp.XpReserve);
                builder.AddField("Level", xp.Level);
                builder.AddField("Next Level", XpManager.GetXp(xp.Level + 1) - xp.Xp);

                await ReplyAsync("", embed: builder.Build());
            } catch {
                await ReplyAsync($"I couldn't look up any Info about {Helper.GetName(user)}, sorry..");
            }
        }

        public static async void TimerCallback() {
            try {
                await ConsoleHelper.Log("Giving away XP to everyone...",
                                LogSeverity.Info);

                foreach (IGuild guild in Cirilla.Client.Guilds) {
                    IEnumerable<IGuildUser> users = await guild.GetUsersAsync();
                    Random rnd = new Random();

                    foreach (IGuildUser user in users.Where(u =>
                            (!u.IsBot) &&
                            (u.Status == UserStatus.Online) &&
                            (u.Status == UserStatus.DoNotDisturb) &&
                            (u.Status == UserStatus.Invisible) &&
                            //TODO: tinker?
                            (u.VoiceChannel != null))) {
                        //Update all [interval] seconds +3 XP
                        XpManager.Update(user, 0, 3);

                        //1 in [GiveRandomXpChance] chance to give user XP
                        if (rnd.Next(0, Information.GiveRandomXpChance) == 0) {
                            int freeXp = 200, freeReserve = 100;
                            XpManager.Update(user, freeXp, freeReserve);
                            if (await guild.GetChannelAsync(guild.DefaultChannelId) is ITextChannel channel) {
                                await ConsoleHelper.Log($"{user} randomly got {freeXp} free XP (1 in {Information.GiveRandomXpChance} chance)",
                                    LogSeverity.Info);
                                await channel.SendMessageAsync($"Lucky you, {user.Mention}! The gods have decided to give you {freeXp} free XP! :moneybag:");
                            }
                        }
                    }
                }

                await ConsoleHelper.Log($"{Information.XpGiveInterval / 1000} Second interval - gave XP to everyone",
                    LogSeverity.Info);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not give XP to everyone! ({ex.Message})", LogSeverity.Error);
            }
        }

        public static void TimerLoop() {
            while (true) {
                Thread.Sleep(Information.XpGiveInterval);
                TimerCallback();
            }
        }


        public static string GetNameForXp() {
			//7 times XP (so "XP" is more common) and 4 times other units
            string[] names = { "XP", "XP", "XP", "XP", "XP", "XP", "XP", "Robux", "Euros", "Schilling", "Bitcoins" };
            return names[Program.Random.Next(0, names.Length + 1)];
        }
    }


    public static class XpManager {
        public static string XpFilePath { get; set; }
        public static XpFile XpInfo { get; set; }

        public static void Init() {
            XpFilePath = Path.Combine(Information.Directory, "userxp.json");
            if (File.Exists(XpFilePath)) {
                XpInfo = JsonConvert.DeserializeObject<XpFile>(File.ReadAllText(XpFilePath));
            } else {
                XpInfo = new XpFile {
                    Users = new List<UserXp>()
                };
                File.WriteAllText(XpFilePath, JsonConvert.SerializeObject(XpInfo));
            }

            new Thread(Xp.TimerLoop).Start();
        }

        //add new user
        public static UserXp Add(IUser user, int xp) {
            UserXp userXp = new UserXp(user.Id, xp, 0);
            XpInfo.Users.Add(userXp);
            WriteOut();
            return userXp;
        }

        public static void Update(IUser user, int plusXp, int plusReserve) {
            foreach (UserXp uxp in XpInfo.Users) {
                if (uxp.UserId == user.Id) {
                    uxp.Xp += plusXp;
                    uxp.XpReserve += plusReserve;
                    WriteOut();
                    return;
                }
            }

            //user does not exist
            Add(user, plusXp);
        }

        public static UserXp Get(IUser user) {
            foreach (UserXp uxp in XpInfo.Users) {
                if (uxp.UserId == user.Id) {
                    return uxp;
                }
            }

            //user does not exist
            return Add(user, 0);
        }

        public static void WriteOut() {
            try {
                //save to file
                File.WriteAllText(XpFilePath, JsonConvert.SerializeObject(XpInfo));
            } catch (Exception ex) {
                ConsoleHelper.Log($"Could not save XP info to XP File! ({ex.Message})", LogSeverity.Error);
            }
        }

        //Get Level for user XP
        public static int GetLevel(int xp) {
            int i = 0;
            while (xp >= GetXp(i)) {
                i++;
            }
            return i - 1;
        }


        //Get required XP for a Level
        public static int GetXp(int level) {
            switch (level) {
                case 1:
                    return 100;
                case 0:
                    return 0;
                default:
                    int previousLevel = GetXp(level - 1);
                    return (int)(previousLevel * Information.XpFactor);
            }
        }


        public class UserXp {
            public UserXp(ulong userId, int xp, int reserve) {
                UserId = userId;
                Xp = xp;
                XpReserve = reserve;
            }

            public ulong UserId { get; set; }
            public int Xp { get; set; }
            public int XpReserve { get; set; }

            [JsonIgnore]
            public int Level => GetLevel(Xp);
        }

        public class XpFile {
            public List<UserXp> Users { get; set; } = new List<UserXp>();
        }
    }
}
