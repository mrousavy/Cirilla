using Discord;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Cirilla {
    public class ConsoleHelper {
        public static Task Log(LogMessage message) {
            switch (message.Severity) {
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
                default:
                    break;
            }
            string text = $"[{DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}] [{message.Severity}] [{message.Source}] {message.Message}";
            Console.WriteLine(text);
            WriteOut(text);

            Console.ResetColor();
            return Task.CompletedTask;
        }
        public static Task Log(string message, LogSeverity logSeverity) {
            Log(new LogMessage(logSeverity, "Print", message));
            return Task.CompletedTask;
        }

        public static void WriteOut(string text) {
            new Thread(() => {
                lock (Helper.Lock) {
                    try {
                        string path = Path.Combine(Information.Directory, "log.txt");
                        if (!File.Exists(path)) {
                            using (File.Create(path)) { }
                        }
                        File.AppendAllLines(path, new List<string>() { text });
                    } catch (Exception ex) {
                        Log($"Could not save Log to Log File! ({ex.Message})", LogSeverity.Error);
                    }
                }
            }).Start();
        }


        public static void Set() {
            //Disable Cursor Visibility
            Console.CursorVisible = false;

            //Window Close & any Close Event
            Handler += DisposeBot;
            SetConsoleCtrlHandler(Handler, true);

            //Ctrl + C | Ctrl + Break
            Console.CancelKeyPress += CancelKey;

            //Disable Quick Edit and Mouse Input
            DisableMouse();
        }

        private static void CancelKey(object sender, ConsoleCancelEventArgs e) {
            DisposeBot(CtrlType.CTRL_C_EVENT);
        }

        private static bool DisposeBot(CtrlType sig) {
            if (Program.Cirilla != null && !Program.Cirilla.IsDisposed)
                Program.Cirilla.Stop().GetAwaiter().GetResult();

            Outro();

            Console.Clear();

            return false;
        }


        private static void DisableMouse() {
            const uint ENABLE_QUICK_EDIT = 0x0040;
            const uint ENABLE_MOUSE_INPUT = 0x0010;

            IntPtr consoleHandle = GetConsoleWindow();

            if (!GetConsoleMode(consoleHandle, out uint consoleMode)) {
                // error
                return;
            }

            // Clear quick edit & Mouse input flags
            consoleMode &= ~ENABLE_QUICK_EDIT;
            consoleMode &= ~ENABLE_MOUSE_INPUT;

            if (!SetConsoleMode(consoleHandle, consoleMode)) {
                // error
            }
        }

        public static void Intro() {
            Console.Clear();

            string introText = "~Cirilla~";
            int left = (Console.WindowWidth / 2) - (introText.Length / 2);
            int top = (Console.WindowHeight / 2) - 1;
            ConsoleColor introColor = ConsoleColor.Magenta;

            Console.SetCursorPosition(left, top);
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = introColor;

            foreach (char ch in introText.ToCharArray()) {
                Console.Write(ch);
                Thread.Sleep(90);
            }

            Console.ForegroundColor = originalColor;

            Thread.Sleep(2300);

            Console.Clear();
        }

        public static void Outro() {
            Console.Clear();

            string introText = "Shutting down..";
            int left = (Console.WindowWidth / 2) - (introText.Length / 2);
            int top = (Console.WindowHeight / 2) - 1;
            ConsoleColor outroColor = ConsoleColor.Red;

            Console.SetCursorPosition(left, top);
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = outroColor;

            foreach (char ch in introText.ToCharArray()) {
                Console.Write(ch);
                Thread.Sleep(50);
            }

            Console.ForegroundColor = originalColor;

            Thread.Sleep(300);
        }

        private enum CtrlType {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private delegate bool EventHandler(CtrlType sig);
        private static EventHandler Handler;


        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();
    }
}
