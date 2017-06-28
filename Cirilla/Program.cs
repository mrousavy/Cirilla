using Cirilla.Services.GuildConfig;
using Cirilla.Services.News;
using Cirilla.Services.Xp;
using Discord;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Cirilla {
    public class Program {
        public static Random Random;

        internal static DateTime StartTime;
        internal static Cirilla Cirilla;

        public static void Main(string[] args) {
            Random = new Random();
            if (!Directory.Exists(Information.Directory)) {
                Directory.CreateDirectory(Information.Directory);
            }

            Information.LoadInfo();
            ConsoleHelper.Log(">> Starting Cirilla Discord Bot..", LogSeverity.Info);

#pragma warning disable 219
            bool skipIntro = false;
#pragma warning restore 219

            foreach (string arg in args) {
                string larg = arg.ToLower();
                switch (larg) {
                    case "skip":
                        // ReSharper disable once RedundantAssignment
                        skipIntro = true;
                        break;
                }
            }

            // disable for non Windowsx86
            ConsoleHelper.Set();

            Console.Title = "Cirilla Discord Bot";

#if !DEBUG
            if (skipIntro)
                ConsoleHelper.Intro();
#endif

            GuildConfigManager.Init();

            Cirilla = new Cirilla(LogSeverity.Debug);
            StartTime = DateTime.Now;

            ConsoleHelper.Log("Initializing services..", LogSeverity.Info);
            Stopwatch sw = Stopwatch.StartNew();
            XpManager.Init();
            NewsService.Init();
            ConsoleHelper.Log($"Finished initializing services! ({sw.ElapsedMilliseconds}ms)", LogSeverity.Info);

            Thread.Sleep(-1);
        }
    }
}