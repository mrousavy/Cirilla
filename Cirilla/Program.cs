using Cirilla.Modules;
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

            // disabled for Raspberry because of DllImports
            //ConsoleHelper.Set();

            Console.Title = "Cirilla Discord Bot";

            if (!skipIntro)
                ConsoleHelper.Intro();

            Cirilla = new Cirilla(Discord.LogSeverity.Debug);
            StartTime = DateTime.Now;
            XpManager.Init();
            News.Init();

            Thread.Sleep(-1);
        }
    }
}