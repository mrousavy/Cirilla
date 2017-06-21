using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Cirilla.Services.Xp {
    public static class XpManager {
        public static XpFile XpInfo { get; set; }

        public static void Init() {
            //get guilds
            string[] directories = Directory.GetDirectories(Information.Directory);
            string[] xpfiles = new string[directories.Length];
            XpInfo = new XpFile();

            if (directories.Length > 0) {
                ConsoleHelper.Log($"Loading {directories.Length} Guild XP Files..", LogSeverity.Info);
                Stopwatch sw = Stopwatch.StartNew();

                for (int i = 0; i < directories.Length; i++) {
                    string directory = directories[i];
                    string xpfile = Path.Combine(directory, "userxp.json");
                    //Regex.Match(directory, "\\\\[0-9]+$").ToString();
                    string guildidstr = Path.GetFileName(Path.GetDirectoryName(xpfile));
                    ulong guildid = ulong.Parse(guildidstr);

                    if (!File.Exists(xpfile)) {
                        GuildXp guildxp = new GuildXp();
                        File.WriteAllText(xpfile, JsonConvert.SerializeObject(guildxp));
                        XpInfo.Guilds.Add(new KeyValuePair<ulong, GuildXp>(guildid, guildxp));
                    } else {
                        GuildXp guildxp = JsonConvert.DeserializeObject<GuildXp>(File.ReadAllText(xpfile));
                        XpInfo.Guilds.Add(new KeyValuePair<ulong, GuildXp>(guildid, guildxp));
                    }
                    xpfiles[i] = xpfile;
                }

                sw.Stop();
                ConsoleHelper.Log(
                    $"Done loading {directories.Length} Guild XP Files! (took {sw.ElapsedMilliseconds}ms)",
                    LogSeverity.Info);
            } else {
                ConsoleHelper.Log("No XP Files stored yet.", LogSeverity.Info);
            }

            new Thread(TimerLoop).Start();
        }


        //Xp Users = XpInfo.Guilds.First(kvp => kvp.Key == guild.Id).Value.Users

        //add new user
        public static UserXp Add(IGuild guild, IUser user, int xp) {
            //contains Guild?
            bool contains = XpInfo.Guilds.Any(kvp => kvp.Key == guild.Id);
            if (!contains) {
                XpInfo.Guilds.Add(new KeyValuePair<ulong, GuildXp>(guild.Id, new GuildXp()));
            }

            UserXp userXp = new UserXp(user.Id, xp);
            XpInfo.Guilds.First(kvp => kvp.Key == guild.Id).Value.Users.Add(userXp);
            WriteOut();
            return userXp;
        }

        public static void Update(IGuild guild, IUser user, int plusXp) {
            foreach (UserXp uxp in XpInfo.Guilds.First(kvp => kvp.Key == guild.Id).Value.Users) {
                if (uxp.UserId == user.Id) {
                    uxp.Xp += plusXp;
                    WriteOut();
                    return;
                }
            }

            //user does not exist
            Add(guild, user, plusXp);
        }

        public static UserXp Get(IGuild guild, IUser user) {
            //LINQ is love <3
            bool contains = XpInfo.Guilds.Where(kvp => kvp.Key == guild.Id)
                .Any(kvp => kvp.Value.Users.Any(userxp => userxp.UserId == user.Id));

            if (contains) {
                foreach (UserXp uxp in XpInfo.Guilds.First(kvp => kvp.Key == guild.Id).Value.Users) {
                    if (uxp.UserId == user.Id) {
                        return uxp;
                    }
                }
            }

            //user does not exist
            return Add(guild, user, 0);
        }

        public static void WriteOut() {
            try {
                foreach (KeyValuePair<ulong, GuildXp> pairs in XpInfo.Guilds) {
                    string directory = Path.Combine(Information.Directory, pairs.Key.ToString());
                    if (!Directory.Exists(directory)) {
                        Directory.CreateDirectory(directory);
                        ConsoleHelper.Log($"Created new Directory for server {pairs.Key}.", LogSeverity.Info);
                    }
                    string xpfile = Path.Combine(directory, "userxp.json");
                    string serialized =
                        JsonConvert.SerializeObject(pairs.Value);
                    File.WriteAllText(xpfile, serialized);
                }
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
                    return (int) (previousLevel * Information.XpFactor);
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
                        !u.IsBot &&
                        (u.Status == UserStatus.Online ||
                         u.Status == UserStatus.DoNotDisturb ||
                         u.Status == UserStatus.Invisible) &&
                        u.VoiceChannel != null)) {
                        //Update all [interval] seconds +3 XP
                        Update(guild, user, 3);
                        receivers.Add(user.ToString());

                        //1 in [GiveRandomXpChance] chance to give user XP
                        if (rnd.Next(0, Information.GiveRandomXpChance) == 0) {
                            const int freeXp = 200;
                            Update(guild, user, freeXp);
                            if (await guild.GetChannelAsync(guild.DefaultChannelId) is ITextChannel channel) {
                                await ConsoleHelper.Log(
                                    $"{user} randomly got {freeXp} free XP (1 in {Information.GiveRandomXpChance} chance)",
                                    LogSeverity.Info);
                                await channel.SendMessageAsync(
                                    $"Lucky you, {user.Mention}! The gods have decided to give you {freeXp} free XP! :moneybag:");
                            }
                        }
                    }
                }

                if (receivers.Count < 1) {
                    await ConsoleHelper.Log(
                        $"{Information.XpGiveInterval / 1000} Second interval - no-one gained XP!",
                        LogSeverity.Info);
                } else {
                    await ConsoleHelper.Log(
                        $"{Information.XpGiveInterval / 1000} Second interval - gave XP to: {string.Join(", ", receivers)}",
                        LogSeverity.Info);
                }
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
        public UserXp(ulong userId, int xp) {
            UserId = userId;
            Xp = xp;
        }

        public ulong UserId { get; set; }
        public int Xp { get; set; }

        [JsonIgnore]
        public int Level => XpManager.GetLevel(Xp);
    }

    public class XpFile {
        public List<KeyValuePair<ulong, GuildXp>> Guilds { get; set; } = new List<KeyValuePair<ulong, GuildXp>>();
    }

    public class GuildXp {
        public List<UserXp> Users { get; set; } = new List<UserXp>();
    }
}