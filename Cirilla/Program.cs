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

            ConsoleHelper.Set();

            if (!skipIntro)
                Intro();

            Cirilla = new Cirilla();

            Thread.Sleep(-1);
        }


        public static void Intro() {
            Console.Title = "Cirilla Discord Bot";

            string introText = "~Cirilla~";
            int left = (Console.WindowWidth / 2) - (introText.Length / 2);
            int top = (Console.WindowHeight / 2) - 1;
            ConsoleColor introColor = ConsoleColor.Cyan;

            Console.SetCursorPosition(left, top);
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = introColor;

            foreach (char ch in introText.ToCharArray()) {
                Console.Write(ch);
                Thread.Sleep(90);
            }

            Console.ForegroundColor = originalColor;

            Thread.Sleep(2500);

            Console.Clear();
        }
    }
}