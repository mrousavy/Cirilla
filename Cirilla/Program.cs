using Cirilla.Modules;
using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;

namespace Cirilla {
    public class Program {
        public static Random Random;

        internal static DateTime StartTime;
        internal static Cirilla Cirilla;

        public static void Main(string[] args) {
            Random = new Random();

            Information.LoadInfo();
            ConsoleHelper.Log(">> Starting Cirilla Discord Bot..", LogSeverity.Info);

            bool skipIntro = false;

            foreach (string arg in args) {
                string larg = arg.ToLower();
                switch (larg) {
                    case "skip":
                        skipIntro = true;
                        break;
                }
            }
#if DEBUG
            skipIntro = true;
#endif

            // disable for non Windowsx86
            ConsoleHelper.Set();

            Console.Title = "Cirilla Discord Bot";

            if (!skipIntro)
                ConsoleHelper.Intro();

            Cirilla = new Cirilla(LogSeverity.Debug);
            StartTime = DateTime.Now;
            XpManager.Init();
            News.Init();

            Thread.Sleep(-1);
        }
    }
}
