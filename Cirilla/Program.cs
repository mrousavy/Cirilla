using Cirilla.Services.News;
using Cirilla.Services.Xp;
using Discord;
using System;
using System.Threading;

namespace Cirilla {
    public class Program {
        public static Random Random;

        internal static DateTime StartTime;
        internal static Cirilla Cirilla;

        public static void Main(string[] args) {
            Random = new Random();

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

            Cirilla = new Cirilla(LogSeverity.Debug);
            StartTime = DateTime.Now;
            XpManager.Init();
            NewsService.Init();

            Thread.Sleep(-1);
        }
    }
}
