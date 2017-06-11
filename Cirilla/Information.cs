using System;

namespace Cirilla {
    internal static class Information {
        //Discord API Infos
        internal const string ClientId = "323123443136593920";
        internal const string ClientSecret = "UnGeUiK-deSuCIwAPSFbzgRLdL43gKhQ";
        internal const string Username = "Cirilla#2111";
        internal const string Token = "MzIzMTIzNDQzMTM2NTkzOTIw.DB2jvA.oMbfXRviQqjQp4N3Km2wisv0VJI";


        //Default Text Channel
        internal const string TextChannel = "general";
        //Bot prefix ($help)
        internal const char Prefix = '$';
        //Bot Source Code Repository URL
        internal const string RepoUrl = "http://github.com/mrousavy/Cirilla";
        //Bot Creator
        internal const string Senpai = "<@266162606161526784>";
        //URL for Bot Profile Pic
        internal const string IconUrl = "http://github.com/mrousavy/Cirilla/Icon";

        //Directory to store configs in
        internal static string Directory = AppContext.BaseDirectory;

        //Interval in ms to give XP (300000 = 5m)
        internal const int XpGiveInterval = 300000;

        //1 in [RandomReactionChance] chance to add a random Emoji as reaction to a new message
        internal const int RandomReactionChance = 150;

        //Time in ms until a votekick expires
        internal const int VotekickExpire = 30000;
        internal const string VotekickYes = "👍";
        internal const string VotekickNo = "👎";
    }
}
