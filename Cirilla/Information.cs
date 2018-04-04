using System;
using System.Diagnostics;
using System.IO;
using Discord;
using Newtonsoft.Json;

namespace Cirilla
{
    internal static class Information
    {
        internal static string GitHubLogo = "https://jitpack.io/w/img/github-logo.png";

        internal static string InviteLink =
            "https://discordapp.com/oauth2/authorize?client_id=323123443136593920&scope=bot&permissions=67184707";

        //Directory to store configs in
        internal static string Directory { get; } = Path.Combine(AppContext.BaseDirectory, "_data");

        internal static bool NeedsWs4Net { get; set; }
        internal static Configuration Config { get; set; }
        internal static LogSeverity LogSeverity => Config.LogSeverity;
        internal static string PokedexUrl => Config.PokedexUrl;
        internal static string Token => Config.Token;
        internal static string PastebinToken => Config.PastebinToken;
        internal static string TextChannel => Config.TextChannel;
        internal static string Botchat => Config.Botchat;
        internal static char Prefix => Config.Prefix;
        internal static string SecondaryPrefix => Config.SecondaryPrefix;
        internal static string RepoUrl => Config.RepoUrl;
        internal static string Owner => Config.Owner;
        internal static ulong OwnerId => Config.OwnerId;
        internal static string IconUrl => Config.IconUrl;
        internal static string BotIconUrl => Config.BotIconUrl;
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
        internal static DateTime LastPost => Config.LastPost;
        internal static string LastArticle => Config.LastArticle;
        internal static int NewsInterval => Config.NewsInterval;
        internal static bool PmHelp => Config.PmHelp;
        internal static long MaxLogSize => Config.MaxLogSize;
        internal static int CompileTimeout => Config.CompileTimeout;
        internal static int ExecutionTimeout => Config.ExecutionTimeout;
        internal static string ThinkEmoji => "🤔";


        public static void LoadInfo()
        {
            //lock object so config does not get written to file (endless loop of errors)
            lock (Helper.Lock)
            {
                string config = Path.Combine(Directory, "config.json");

                ConsoleHelper.Log("Loading config..", LogSeverity.Info);
                ConsoleHelper.Log($"pwd: {Directory}", LogSeverity.Info);

                if (!File.Exists(config))
                {
                    File.WriteAllText(config, JsonConvert.SerializeObject(new Configuration()));
                    ConsoleHelper.Log($"No configuration set, please edit {config}!", LogSeverity.Critical);
                    Console.ReadKey();
                    Process.GetCurrentProcess().Kill();
                } else
                {
                    try
                    {
                        Config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(config));
                    } catch (Exception ex)
                    {
                        ConsoleHelper.Log(ex.Message, LogSeverity.Critical);
                        ConsoleHelper.Log($"Could not load config.json!, please edit {config}!", LogSeverity.Critical);
                        Console.ReadKey();
                        Process.GetCurrentProcess().Kill();
                    }
                }

                if (Config.Token == "DISCORD_API_TOKEN_GOES_HERE")
                {
                    ConsoleHelper.Log($"Discord API Token is invalid, please edit {config}!", LogSeverity.Critical);
                    Console.ReadKey();
                    Process.GetCurrentProcess().Kill();
                }
            }
        }


        public static void WriteOut()
        {
            string config = Path.Combine(Directory, "config.json");
            File.WriteAllText(config, JsonConvert.SerializeObject(Config));
        }
    }


    public class Configuration
    {
        public bool AllowVotekick = false;

        //Default Botchat Text Channel
        public string Botchat = "botchat";

        //URL for Bot Pic
        public string BotIconUrl = "https://raw.githubusercontent.com/mrousavy/Cirilla/master/Resources/Bot.png";

        //Time in milliseconds until the compilation of $exec scipts cancels
        public int CompileTimeout = 15000;

        //Time in milliseconds until the execution of $exec scripts cancels
        public int ExecutionTimeout = 5000;

        //1 in [GiveRandomXpChance] to give a user 200 XP on XpGiveInterval interval
        public int GiveRandomXpChance = 400;

        //URL for Bot Profile Pic
        public string IconUrl = "https://raw.githubusercontent.com/mrousavy/Cirilla/master/Resources/Ciri_round.png";

        //Last article (name) that got posted
        public string LastArticle;

        //Last time news got posted
        public DateTime LastPost = DateTime.Now;

        //Minimum Log Severity to output/log
        public LogSeverity LogSeverity = LogSeverity.Info;

        //Maximum days for reminders
        public int MaximumRemindDays = 5;

        //Maximum simultaneous reminders for a user
        public int MaximumReminders = 3;

        //Max Log Size in bytes
        public long MaxLogSize = 1024 * 1024 * 10;

        //Time (in hours) until next news article is sent
        public int NewsInterval = 24;

        //Bot Creator/Owner (me)
        public string Owner = "<@266162606161526784>";

        //Bot Creator/Owner ID (me)
        public ulong OwnerId = 266162606161526784;

        //1 in [OwnXp] go to the XP Giver
        public int OwnXp = 7;

        //Pastebin Token for sending Log
        public string PastebinToken = "PASTEBIN_API_TOKEN_GOES_HERE";

        //Send Help in private Message
        public bool PmHelp = true;

        public string PokedexUrl =
            "https://vignette.wikia.nocookie.net/pokemon/images/6/6f/Pok%C3%A9dex_Pt.png/revision/latest/scale-to-width-down/2000?cb=20110528144645";

        //Bot prefix ($help)
        public char Prefix = '$';

        //1 in [RandomReactionChance] chance to add a random Emoji as reaction to a new message
        public int RandomReactionChance = 100;

        //Bot Source Code Repository URL
        public string RepoUrl = "http://github.com/mrousavy/Cirilla";

        //Bot prefix (!help)
        public string SecondaryPrefix = "!";

        //Default Text Channel
        public string TextChannel = "general";

        //Discord API Info
        public string Token = "DISCORD_API_TOKEN_GOES_HERE";

        //Time in ms until a votekick expires
        public int VotekickExpire = 30000;

        public string VotekickNo = "👎";

        public string VotekickYes = "👍";

        //Formula for calculating XP/Level: Level = PreviousLevel * XpFactor
        public double XpFactor = 1.20;


        //Interval in ms to give XP (2100000 = 35m)
        public int XpGiveInterval = 2100000;
    }
}