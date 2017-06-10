using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Xp : ModuleBase {
        [Command("xp"), Summary("Give a user XP")]
        public async Task Give(IUser user, int xp) {
            try {
                XpManager.UserXp userxp = XpManager.Get(Context.User);
                if (userxp.Xp >= xp) {
                    XpManager.Update(user, xp);
                    await ReplyAsync($"{Context.User.Mention} donated {user.Mention} {xp} XP!");
                } else {
                    await ReplyAsync($"You can't give {xp} XP to {user.Username}, you only have {userxp.Xp} XP!");
                }
            } catch (Exception ex) {
                await ReplyAsync($"Error! {ex.Message}");
            }
        }

        [Command("xp"), Summary("Show own XP")]
        public async Task Info() {
            try {
                XpManager.UserXp xp = XpManager.Get(Context.User);
                await ReplyAsync($"{Context.User.Mention}'s XP: {xp.Xp}");
            } catch (Exception ex) {
                await ReplyAsync($"Error! {ex.Message}");
            }
        }

        public static async void TimerCallback(object state) {
            DiscordSocketClient client = state as DiscordSocketClient;
            if (client == null)
                return;

            foreach (IGuild guild in client.Guilds) {
                IEnumerable<IGuildUser> users = await guild.GetUsersAsync();

                foreach (IUser user in users) {
                    XpManager.Update(user, 10);
                }
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
                File.WriteAllText(XpFilePath, "{}");
                Xp = new XpFile {
                    Users = new List<UserXp>()
                };
            }
        }

        //add new user
        public static UserXp Add(IUser user, int xp) {
            UserXp userXp = new UserXp(user, xp);
            Xp.Users.Add(userXp);
            WriteOut();
            return userXp;
        }

        public static void Update(IUser user, int plusXp) {
            for (int i = 0; i < Xp.Users.Count; i++) {
                UserXp uxp = Xp.Users[i];
                if (uxp.UserId == user.Id) {
                    uxp.Xp += plusXp;
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
            //save to file
            File.WriteAllText(XpFilePath, JsonConvert.SerializeObject(Xp));
        }



        public struct UserXp {
            public UserXp(IUser user, int xp) {
                UserId = user.Id;
                Xp = xp;
            }

            public ulong UserId { get; set; }
            public int Xp { get; set; }
        }
        public class XpFile {
            public List<UserXp> Users { get; set; } = new List<UserXp>();
        }
    }
}
