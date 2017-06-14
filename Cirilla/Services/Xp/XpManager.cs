using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Cirilla.Services.Xp {

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

            new Thread(TimerLoop).Start();
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


        public static async void TimerCallback() {
            try {
                await ConsoleHelper.Log("Giving away XP to everyone...",
                                LogSeverity.Info);

                List<string> receivers = new List<string>();

                foreach (IGuild guild in Cirilla.Client.Guilds) {
                    IEnumerable<IGuildUser> users = await guild.GetUsersAsync();
                    Random rnd = new Random();

                    foreach (IGuildUser user in users.Where(u =>
                            (!u.IsBot) &&
                            (u.Status == UserStatus.Online ||
                            u.Status == UserStatus.DoNotDisturb ||
                            u.Status == UserStatus.Invisible) &&
                            //TODO: tinker?
                            (u.VoiceChannel != null))) {
                        //Update all [interval] seconds +3 XP
                        XpManager.Update(user, 0, 3);
                        receivers.Add(user.ToString());

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

                await ConsoleHelper.Log($"{Information.XpGiveInterval / 1000} Second interval - gave XP to: {string.Join(", ", receivers)}",
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
        public int Level => XpManager.GetLevel(Xp);
    }

    public class XpFile {
        public List<UserXp> Users { get; set; } = new List<UserXp>();
    }
}
