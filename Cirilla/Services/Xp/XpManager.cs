using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Cirilla.Services.GuildConfig;
using Discord;
using Newtonsoft.Json;

namespace Cirilla.Services.Xp
{
    public static class XpManager
    {
        public static XpFile XpInfo { get; set; }

        public static void Init()
        {
            //get guilds
            var directories = Directory.GetDirectories(Information.Directory);
            var xpfiles = new string[directories.Length];
            XpInfo = new XpFile();

            if (directories.Length > 0)
            {
                ConsoleHelper.Log($"Loading {directories.Length} Guild XP Files..", LogSeverity.Info);
                var sw = Stopwatch.StartNew();

                for (int i = 0; i < directories.Length; i++)
                {
                    string directory = directories[i];
                    string xpfile = Path.Combine(directory, "userxp.json");
                    //Regex.Match(directory, "\\\\[0-9]+$").ToString();
                    string guildidstr = Path.GetFileName(Path.GetDirectoryName(xpfile));
                    ulong guildid = ulong.Parse(guildidstr);

                    if (!GuildConfigManager.Get(guildid).EnableXpSystem) continue;

                    if (!File.Exists(xpfile))
                    {
                        var guildxp = new GuildXp();
                        File.WriteAllText(xpfile, JsonConvert.SerializeObject(guildxp));
                        XpInfo.Guilds.Add(new KeyValuePair<ulong, GuildXp>(guildid, guildxp));
                    } else
                    {
                        var guildxp = JsonConvert.DeserializeObject<GuildXp>(File.ReadAllText(xpfile));
                        XpInfo.Guilds.Add(new KeyValuePair<ulong, GuildXp>(guildid, guildxp ?? new GuildXp()));
                    }

                    xpfiles[i] = xpfile;
                }

                sw.Stop();
                ConsoleHelper.Log(
                    $"Done loading {directories.Length} Guild XP Files! (took {sw.ElapsedMilliseconds}ms)",
                    LogSeverity.Info);
            } else
            {
                ConsoleHelper.Log("No XP Files stored yet.", LogSeverity.Info);
            }

            new Thread(TimerLoop).Start();
        }


        //add new user
        public static UserXp Add(IGuild guild, IUser user, int xp)
        {
            //contains Guild?
            bool contains = XpInfo.Guilds.Any(kvp => kvp.Key == guild.Id);
            if (!contains) XpInfo.Guilds.Add(new KeyValuePair<ulong, GuildXp>(guild.Id, new GuildXp()));

            var userXp = new UserXp(user.Id, xp);
            XpInfo.Guilds.First(kvp => kvp.Key == guild.Id).Value.Users.Add(userXp);
            WriteOut();
            return userXp;
        }

        public static void Update(IGuild guild, IUser user, int plusXp)
        {
            foreach (var uxp in XpInfo.Guilds.First(kvp => kvp.Key == guild.Id).Value.Users)
            {
                if (uxp.UserId == user.Id)
                {
                    uxp.Xp += plusXp;
                    WriteOut();
                    return;
                }
            }

            //user does not exist
            Add(guild, user, plusXp);
        }

        public static UserXp Get(IGuild guild, IUser user)
        {
            //LINQ is love <3
            bool contains = XpInfo.Guilds.Where(kvp => kvp.Key == guild.Id)
                .Any(kvp => kvp.Value.Users.Any(userxp => userxp.UserId == user.Id));


            if (contains)
                foreach (var uxp in XpInfo.Guilds.First(kvp => kvp.Key == guild.Id).Value.Users)
                {
                    if (uxp.UserId == user.Id)
                        return uxp;
                }

            //user does not exist
            return Add(guild, user, 0);
        }

        public static List<UserXp> Get(IGuild guild)
        {
            return XpInfo.Guilds.First(kvp => kvp.Key == guild.Id).Value.Users;
        }

        public static List<UserXp> Top10(IGuild guild)
        {
            //LINQ is love <3
            var gxp = XpInfo.Guilds.First(kvp => kvp.Key == guild.Id).Value;
            var userxp = gxp.Users.OrderByDescending(uxp => uxp).ToList();

            return userxp;
        }

        public static void WriteOut()
        {
            try
            {
                foreach (var pair in XpInfo.Guilds)
                {
                    string directory = Path.Combine(Information.Directory, pair.Key.ToString());
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        ConsoleHelper.Log($"Created new Directory for server {pair.Key}.", LogSeverity.Info);
                    }

                    string xpfile = Path.Combine(directory, "userxp.json");
                    string serialized =
                        JsonConvert.SerializeObject(pair.Value);
                    File.WriteAllText(xpfile, serialized);
                }
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Could not save XP info to XP File! ({ex.Message})", LogSeverity.Error);
            }
        }

        public static void RemoveUser(IGuild guild, IUser user)
        {
            try
            {
                var guildXp = XpInfo.Guilds.First(kvp => kvp.Key == guild.Id).Value;
                guildXp.Users.RemoveAll(p => p.UserId == user.Id);
                WriteOut();
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Could not remove User from XP File! ({ex.Message})", LogSeverity.Error);
            }
        }

        //Get Level for user XP
        public static int GetLevel(int xp)
        {
            int i = 0;
            while (xp >= GetXp(i))
            {
                i++;
            }

            return i - 1;
        }


        //Get required XP for a Level
        public static int GetXp(int level)
        {
            switch (level)
            {
                case 1:
                    return 100;
                case 0:
                    return 0;
                default:
                    int previousLevel = GetXp(level - 1);
                    return (int) (previousLevel * Information.XpFactor);
            }
        }


        public static async void TimerCallback()
        {
            try
            {
                ConsoleHelper.Log("Giving away XP to everyone...",
                    LogSeverity.Info);

                var receivers = new List<string>();

                foreach (IGuild guild in Cirilla.Client.Guilds)
                {
                    if (!GuildConfigManager.Get(guild.Id).EnableXpSystem) continue;

                    IEnumerable<IGuildUser> users = await guild.GetUsersAsync();
                    var rnd = new Random();

                    foreach (var user in users.Where(u =>
                        !u.IsBot &&
                        (u.Status == UserStatus.Online ||
                         u.Status == UserStatus.DoNotDisturb ||
                         u.Status == UserStatus.Invisible) &&
                        u.VoiceChannel != null))
                    {
                        //Update all [interval] seconds +3 XP
                        Update(guild, user, 3);
                        receivers.Add(user.ToString());

                        //1 in [GiveRandomXpChance] chance to give user XP
                        if (rnd.Next(0, Information.GiveRandomXpChance) == 0)
                        {
                            const int freeXp = 200;
                            Update(guild, user, freeXp);
                            if (await guild.GetChannelAsync(guild.DefaultChannelId) is ITextChannel channel)
                            {
                                ConsoleHelper.Log(
                                    $"{user} randomly got {freeXp} free XP (1 in {Information.GiveRandomXpChance} chance)",
                                    LogSeverity.Info);
                                await channel.SendMessageAsync(
                                    $"Lucky you, {user.Mention}! The gods have decided to give you {freeXp} free XP! :moneybag:");
                            }
                        }
                    }
                }

                if (receivers.Count < 1)
                    ConsoleHelper.Log(
                        $"{Information.XpGiveInterval / 1000} Second interval - no-one gained XP!",
                        LogSeverity.Info);
                else
                    ConsoleHelper.Log(
                        $"{Information.XpGiveInterval / 1000} Second interval - gave XP to: {string.Join(", ", receivers)}",
                        LogSeverity.Info);
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Could not give XP to everyone! ({ex.Message})", LogSeverity.Error);
            }
        }

        public static void TimerLoop()
        {
            while (true)
            {
                Thread.Sleep(Information.XpGiveInterval);
                TimerCallback();
            }
        }
    }


    public class UserXp : IComparable
    {
        public UserXp(ulong userId, int xp)
        {
            UserId = userId;
            Xp = xp;
        }

        public ulong UserId { get; set; }
        public int Xp { get; set; }

        [JsonIgnore]
        public int Level => XpManager.GetLevel(Xp);

        public int CompareTo(object obj)
        {
            if (obj is UserXp xp2)
            {
                if (Xp > xp2.Xp)
                    return 1;
                if (Xp == xp2.Xp)
                    return 0;
            }

            return -1;
        }
    }

    public class XpFile
    {
        public List<KeyValuePair<ulong, GuildXp>> Guilds { get; set; } = new List<KeyValuePair<ulong, GuildXp>>();
    }

    public class GuildXp
    {
        public List<UserXp> Users { get; set; } = new List<UserXp>();
    }
}