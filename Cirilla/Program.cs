using Cirilla.Modules;
using System;
using System.Threading;

namespace Cirilla {
    public class Program {
        internal static Cirilla Cirilla;

        public static void Main(string[] args) {
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

            ConsoleHelper.Set();
            Console.Title = "Cirilla Discord Bot";

            XpManager.Init();

            if (!skipIntro)
                ConsoleHelper.Intro();

            Cirilla = new Cirilla(Discord.LogSeverity.Debug);

            Thread.Sleep(-1);
        }
    }
}