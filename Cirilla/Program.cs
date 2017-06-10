using System;
using System.Threading;

namespace Cirilla {
    public class Program {
        internal static Cirilla Cirilla;

        public static void Main(string[] args) {
            Thread.Sleep(3000);

            ConsoleHelper.Set();
            Intro();

            Cirilla = new Cirilla();

            Thread.Sleep(-1);
        }


        public static void Intro() {
            Console.Title = "Cirilla Discord Bot";

            string introText = "~Cirilla~";
            int left = (Console.WindowWidth / 2) - (introText.Length / 2);
            int top = (Console.WindowHeight / 2);
            Console.SetCursorPosition(left, top);
            ConsoleColor color = Console.ForegroundColor = ConsoleColor.Cyan;

            foreach (char ch in introText.ToCharArray()) {
                Console.Write(ch);
                Thread.Sleep(90);
            }


            Console.ForegroundColor = color;

            Thread.Sleep(2500);

            Console.Clear();
        }
    }
}