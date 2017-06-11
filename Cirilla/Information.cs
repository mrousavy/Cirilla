using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace Cirilla {
    internal static class Information {
        //Directory to store configs in
        internal static string Directory { get; set; } = AppContext.BaseDirectory;

        internal static Config Config { get; set; }

        internal static string ClientId => Config.ClientId;
        internal static string ClientSecret => Config.ClientSecret;
        internal static string Username => Config.Username;
        internal static string Token => Config.Token;
        internal static string TextChannel => Config.TextChannel;
        internal static char Prefix => Config.Prefix;
        internal static string RepoUrl => Config.RepoUrl;
        internal static string Senpai => Config.Senpai;
        internal static string IconUrl => Config.IconUrl;

        internal static int XpGiveInterval => Config.XpGiveInterval;
        internal static int RandomReactionChance => Config.RandomReactionChance;

        internal static int VotekickExpire => Config.VotekickExpire;
        internal static string VotekickYes => Config.VotekickYes;
        internal static string VotekickNo => Config.VotekickNo;
        internal static bool AllowVotekick => Config.AllowVotekick;



        public static void LoadInfo() {
            string config = Path.Combine(Directory, "config.json");
            if (!File.Exists(config)) {
                File.WriteAllText(config, JsonConvert.SerializeObject(new Config()));
                ConsoleHelper.Log($"No configuration set, please edit {config}!", Discord.LogSeverity.Critical);
                Console.ReadKey();
                Process.GetCurrentProcess().Kill();
            } else {
                try {
                    Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(config));
                } catch (Exception ex) {
                    ConsoleHelper.Log(ex.Message, Discord.LogSeverity.Critical);
                    ConsoleHelper.Log($"Could not load config.json!, please edit {config}!", Discord.LogSeverity.Critical);
                    Console.ReadKey();
                    Process.GetCurrentProcess().Kill();
                }
            }
        }
    }


    public class Config {
        //Discord API Infos
        public string ClientId = "323123443136593920";
        public string ClientSecret = "<YourDiscordApiClientSecret>";
        public string Username = "Cirilla#2111";
        public string Token = "<YourDiscordApiToken>";


        //Default Text Channel
        public string TextChannel = "general";
        //Bot prefix ($help)
        public char Prefix = '$';
        //Bot Source Code Repository URL
        public string RepoUrl = "http://github.com/mrousavy/Cirilla";
        //Bot Creator (me)
        public string Senpai = "<@266162606161526784>";
        //URL for Bot Profile Pic
        public string IconUrl = "http://github.com/mrousavy/Cirilla/Icon";


        //Interval in ms to give XP (300000 = 5m)
        public int XpGiveInterval = 300000;

        //1 in [RandomReactionChance] chance to add a random Emoji as reaction to a new message
        public int RandomReactionChance = 150;

        //Time in ms until a votekick expires
        public int VotekickExpire = 30000;
        public string VotekickYes = "👍";
        public string VotekickNo = "👎";
        public bool AllowVotekick = true;
    }
}
