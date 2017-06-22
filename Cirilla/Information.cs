using Discord;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace Cirilla {
    internal static class Information {
        //Directory to store configs in
        internal static string Directory { get; } = Path.Combine(AppContext.BaseDirectory, "_data");
        internal static Configuration Config { get; set; }
        internal static LogSeverity LogSeverity => Config.LogSeverity;
        internal static string Token => Config.Token;
        internal static string TextChannel => Config.TextChannel;
        internal static string Botchat => Config.Botchat;
        internal static char Prefix => Config.Prefix;
        internal static string SecondaryPrefix => Config.SecondaryPrefix;
        internal static string RepoUrl => Config.RepoUrl;
        internal static string Owner => Config.Owner;
        internal static string IconUrl => Config.IconUrl;
        internal static int XpGiveInterval => Config.XpGiveInterval;
        internal static int OwnXp => Config.OwnXp;
        internal static int GiveRandomXpChance => Config.GiveRandomXpChance;
        internal static int RandomReactionChance => Config.RandomReactionChance;
        internal static int VotekickExpire => Config.VotekickExpire;
        internal static string VotekickYes => Config.VotekickYes;
        internal static string VotekickNo => Config.VotekickNo;
        internal static bool AllowVotekick => Config.AllowVotekick;
        internal static double XpFactor => Config.XpFactor;
        internal static int MaximumReminders => Config.MaximumReminders;
        internal static int MaximumRemindDays => Config.MaximumRemindDays;
        internal static string GitHubLogo = "https://jitpack.io/w/img/github-logo.png";
        internal static string InviteLink =
            "https://discordapp.com/oauth2/authorize?client_id=323123443136593920&scope=bot&permissions=67184707";
        internal static DateTime LastPost => Config.LastPost;
        internal static string LastArticle => Config.LastArticle;
        internal static int NewsInterval => Config.NewsInterval;
        internal static bool PmHelp => Config.PmHelp;
        internal static long MaxLogSize => Config.MaxLogSize;
        internal static bool AllowScripts => Config.AllowScripts;
        internal static int CompileTimeout => Config.CompileTimeout;
        internal static int ExecutionTimeout => Config.ExecutionTimeout;


        public static void LoadInfo() {
            //lock object so config does not get written to file (endless loop of errors)
            lock (Helper.Lock) {
                string config = Path.Combine(Directory, "config.json");

                ConsoleHelper.Log("Loading config..", LogSeverity.Info);
                ConsoleHelper.Log($"pwd: {Directory}", LogSeverity.Info);

                if (!File.Exists(config)) {
                    File.WriteAllText(config, JsonConvert.SerializeObject(new Configuration()));
                    ConsoleHelper.Log($"No configuration set, please edit {config}!", LogSeverity.Critical);
                    Console.ReadKey();
                    Process.GetCurrentProcess().Kill();
                } else {
                    try {
                        Config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(config));
                    } catch (Exception ex) {
                        ConsoleHelper.Log(ex.Message, LogSeverity.Critical);
                        ConsoleHelper.Log($"Could not load config.json!, please edit {config}!", LogSeverity.Critical);
                        Console.ReadKey();
                        Process.GetCurrentProcess().Kill();
                    }
                }
            }
        }


        public static void WriteOut() {
            string config = Path.Combine(Directory, "config.json");
            File.WriteAllText(config, JsonConvert.SerializeObject(Config));
        }
    }


    public class Configuration {
        //Discord API Info
        public string Token = "DISCORD_API_TOKEN_GOES_HERE";

        //Minimum Log Severity to output/log
        public LogSeverity LogSeverity = LogSeverity.Info;

        //Default Text Channel
        public string TextChannel = "general";

        //Default Botchat Text Channel
        public string Botchat = "botchat";

        //Bot prefix ($help)
        public char Prefix = '$';

        //Bot prefix (!help)
        public string SecondaryPrefix = "!";

        //Bot Source Code Repository URL
        public string RepoUrl = "http://github.com/mrousavy/Cirilla";

        //Bot Creator/Owner (me)
        public string Owner = "<@266162606161526784>";

        //URL for Bot Profile Pic
        public string IconUrl = "https://raw.githubusercontent.com/mrousavy/Cirilla/master/Resources/Ciri_round.png";


        //Interval in ms to give XP (2100000 = 35m)
        public int XpGiveInterval = 2100000;

        //1 in [OwnXp] go to the XP Giver
        public int OwnXp = 7;

        //Formula for calculating XP/Level: Level = PreviousLevel * XpFactor
        public double XpFactor = 1.20;

        //1 in [GiveRandomXpChance] to give a user 200 XP on XpGiveInterval interval
        public int GiveRandomXpChance = 400;

        //1 in [RandomReactionChance] chance to add a random Emoji as reaction to a new message
        public int RandomReactionChance = 200;

        //Time in ms until a votekick expires
        public int VotekickExpire = 30000;

        public string VotekickYes = "👍";
        public string VotekickNo = "👎";
        public bool AllowVotekick = false;

        //Maximum simultaneous reminders for a user
        public int MaximumReminders = 3;
        //Maximum days for reminders
        public int MaximumRemindDays = 5;

        //Last time news got posted
        public DateTime LastPost = DateTime.Now;

        //Last article (name) that got posted
        public string LastArticle;

        //Time (in hours) until next news article is sent
        public int NewsInterval = 24;

        //Send Help in private Message
        public bool PmHelp = true;

        //Max Log Size in bytes
        public long MaxLogSize = 1024 * 1024 * 10;

        //Enable Roslyn Scripting using the $exec Command
        public bool AllowScripts = true;

        //Time in milliseconds until the compilation of $exec scipts cancels
        public int CompileTimeout = 15000;

        //Time in milliseconds until the execution of $exec scripts cancels
        public int ExecutionTimeout = 5000;
    }
}