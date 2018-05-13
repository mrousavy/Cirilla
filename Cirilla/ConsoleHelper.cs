using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Discord;

namespace Cirilla
{
    public class ConsoleHelper
    {
        public enum CtrlType
        {
            CtrlCEvent = 0
        }

        private static EventHandler _handler;
        public static bool ShuttingDown { get; set; }

        public static void Log(LogMessage message, bool writeOut = true)
        {
            if (ShuttingDown)
                return;

            if (Information.Config != null && Information.LogSeverity < message.Severity)
                return;

            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            string text =
                $"[{DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}] [{message.Severity}] [{message.Source}] {message.Message}";
            Console.WriteLine(text);

            if (writeOut)
                WriteOut(text);

            Console.ResetColor();
        }

        public static void Log(string message, LogSeverity logSeverity)
        {
            Log(new LogMessage(logSeverity, "Print", message));
        }

        public static void WriteOut(string text)
        {
            new Thread(() =>
            {
                lock (Helper.Lock)
                {
                    try
                    {
                        string logfile = Path.Combine(Information.Directory, "log.txt");
                        if (!File.Exists(logfile))
                            using (File.Create(logfile))
                            { }

                        // > than 10 MB (default) -> clear log
                        if (Information.Config != null && new FileInfo(logfile).Length > Information.MaxLogSize)
                            File.WriteAllBytes(logfile, new byte[0]);

                        File.AppendAllLines(logfile, new List<string> { text });
                    } catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Could not save Log to Log File! ({ex.Message})");
                        Console.ResetColor();
                    }
                }
            }).Start();
        }

        public static void Set()
        {
            //Disable Cursor Visibility
            Console.CursorVisible = false;

            //Window Close & any Close Event
            _handler += DisposeBot;
            SetConsoleCtrlHandler(_handler, true);

            //Ctrl + C | Ctrl + Break
            Console.CancelKeyPress += CancelKey;

            //Disable Quick Edit and Mouse Input
            DisableMouse();
        }

        private static void CancelKey(object sender, ConsoleCancelEventArgs e)
        {
            DisposeBot(CtrlType.CtrlCEvent);
        }

        public static bool DisposeBot(CtrlType sig)
        {
            if (Program.Cirilla != null)
                Program.Cirilla.Stop().GetAwaiter().GetResult();

            Outro();

            Console.Clear();

            return false;
        }


        private static void DisableMouse()
        {
            const uint enableQuickEdit = 0x0040;
            const uint enableMouseInput = 0x0010;

            var consoleHandle = GetConsoleWindow();

            if (!GetConsoleMode(consoleHandle, out uint consoleMode)) return;

            // Clear quick edit & Mouse input flags
            consoleMode &= ~enableQuickEdit;
            consoleMode &= ~enableMouseInput;

            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // error
            }
        }

        public static void Intro()
        {
            Console.Clear();

            const string introText = "~Cirilla~";
            int left = Console.WindowWidth / 2 - introText.Length / 2;
            int top = Console.WindowHeight / 2 - 1;
            const ConsoleColor introColor = ConsoleColor.Magenta;

            Console.SetCursorPosition(left, top);
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = introColor;

            foreach (char ch in introText)
            {
                Console.Write(ch);
                Thread.Sleep(90);
            }

            Console.ForegroundColor = originalColor;

            Thread.Sleep(2300);

            Console.Clear();
        }

        public static void Outro()
        {
            ShuttingDown = true;

            Console.Clear();

            const string introText = "Shutting down..";
            int left = Console.WindowWidth / 2 - introText.Length / 2;
            int top = Console.WindowHeight / 2 - 1;
            const ConsoleColor outroColor = ConsoleColor.Red;

            Console.SetCursorPosition(left, top);
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = outroColor;

            foreach (char ch in introText)
            {
                Console.Write(ch);
                Thread.Sleep(50);
            }

            Console.ForegroundColor = originalColor;

            Thread.Sleep(300);
        }


        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        private delegate bool EventHandler(CtrlType sig);
    }
}