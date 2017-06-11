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
                IGuildUser guilduser = Context.User as IGuildUser;
                if (xp < 1 && !guilduser.GuildPermissions.Administrator) {
                    await ReplyAsync("Nice try, you can't drain XP from Users!");
                    return;
                }
                int ownXp = xp / Information.OwnXp;

                XpManager.UserXp userxp = XpManager.Get(Context.User);

                if (userxp.XpReserve >= xp) {
                    XpManager.Update(user, xp, 0);

                    int lvlBefore = XpManager.Get(user).Level;
                    //Drain XP from Sender, minus own XP (you get 1/100 from donations)
                    XpManager.Update(Context.User, ownXp, -xp);
                    int lvlAfter = XpManager.Get(user).Level;

                    await ReplyAsync($"{Helper.GetName(Context.User)} donated {Helper.GetName(user)} {xp} XP! :money_with_wings:");

                    if (lvlAfter > lvlBefore) {
                        await ReplyAsync($":tada: {Helper.GetName(user)} was promoted to level {lvlAfter}! :tada:");
                    }
                } else {
                    await ReplyAsync($"You can't give {xp} XP to {Helper.GetName(user)}, you only have {userxp.XpReserve} XP!");
                }
            } catch {
                await ReplyAsync("Whoops, I couldn't give XP to that user!");
            }
        }

        [Command("xp"), Summary("Show own XP")]
        public async Task Info() {
            try {
                XpManager.UserXp xp = XpManager.Get(Context.User);

                EmbedBuilder builder = new EmbedBuilder {
                    Author = new EmbedAuthorBuilder {
                        Name = Context.User.IsBot ? $"{Context.User}'s XP (Bot)" : $"{Context.User}'s XP",
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
        public async Task Info([Summary("The user you want to display XP")]IUser user) {
            try {
                XpManager.UserXp xp = XpManager.Get(user);

                EmbedBuilder builder = new EmbedBuilder {
                    Author = new EmbedAuthorBuilder {
                        Name = user.IsBot ? $"{user}'s XP (Bot)" : $"{user}'s XP",
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
            await ConsoleHelper.Log($"Giving away XP to everyone...",
                            LogSeverity.Info);

            foreach (IGuild guild in Cirilla.Client.Guilds) {
                IEnumerable<IGuildUser> users = await guild.GetUsersAsync();

                foreach (IUser user in users.Where(u =>
                        (!u.IsBot) &&
                        (u.Status != UserStatus.Offline))) {
                    //Update all [interval] seconds +1 XP
                    XpManager.Update(user, 0, 3);
                }
            }

            await ConsoleHelper.Log($"{Information.XpGiveInterval / 1000} Second interval - gave XP to everyone",
                LogSeverity.Info);
        }

        public static void TimerLoop() {
            while (true) {
                Thread.Sleep(Information.XpGiveInterval);
                TimerCallback();
            }
        }
    }


    public static class XpManager {
        public static string XpFilePath { get; set; }
        public static XpFile Xp { get; set; }

        public static void Init() {
            XpFilePath = Path.Combine(Information.Directory, "userxp.json");
            if (File.Exists(XpFilePath)) {
                Xp = JsonConvert.DeserializeObject<XpFile>(File.ReadAllText(XpFilePath));
            } else {
                Xp = new XpFile {
                    Users = new List<UserXp>()
                };
                File.WriteAllText(XpFilePath, JsonConvert.SerializeObject(Xp));
            }

            new Thread(Modules.Xp.TimerLoop).Start();
        }

        //add new user
        public static UserXp Add(IUser user, int xp) {
            UserXp userXp = new UserXp(user.Id, xp, 0);
            Xp.Users.Add(userXp);
            WriteOut();
            return userXp;
        }

        public static void Update(IUser user, int plusXp, int plusReserve) {
            for (int i = 0; i < Xp.Users.Count; i++) {
                if (Xp.Users[i].UserId == user.Id) {
                    Xp.Users[i].Xp += plusXp;
                    Xp.Users[i].XpReserve += plusReserve;
                    WriteOut();
                    return;
                }
            }

            //user does not exist
            Add(user, plusXp);
        }

        public static UserXp Get(IUser user) {
            foreach (UserXp uxp in Xp.Users) {
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
                File.WriteAllText(XpFilePath, JsonConvert.SerializeObject(Xp));
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
            if (level == 1) {
                return 100;
            } else if (level == 0) {
                return 0;
            } else {
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
