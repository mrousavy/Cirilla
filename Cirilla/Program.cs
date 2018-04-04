using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Cirilla.Services.GuildConfig;
using Cirilla.Services.News;
using Cirilla.Services.Xp;
using Discord;

namespace Cirilla
{
    public class Program
    {
        public static Random Random;

        internal static DateTime StartTime;
        internal static Cirilla Cirilla;

        public static void Main(string[] args)
        {
            try
            {
                Random = new Random();
                if (!Directory.Exists(Information.Directory)) Directory.CreateDirectory(Information.Directory);

                Information.LoadInfo();
                ConsoleHelper.Log(">> Starting Cirilla Discord Bot..", LogSeverity.Info);

#pragma warning disable 219
                bool skipIntro = false;
#pragma warning restore 219

                foreach (string arg in args)
                {
                    string cleanArg = arg.ToLower().Replace("-", "");
                    switch (cleanArg)
                    {
                        case "skip":
                            // ReSharper disable once RedundantAssignment
                            skipIntro = true;
                            break;
                        case "legacy":
                            // Need legacy Sockets (WS4NET)
                            Information.NeedsWs4Net = true;
                            break;
                    }
                }

                // disable for non Windowsx86
                ConsoleHelper.Set();

                Console.Title = "Cirilla Discord Bot";

#if !DEBUG
            if (!skipIntro)
                ConsoleHelper.Intro();
#endif

                GuildConfigManager.Init();

                Cirilla = new Cirilla(LogSeverity.Debug);
                StartTime = DateTime.Now;

                ConsoleHelper.Log("Initializing services..", LogSeverity.Info);
                var sw = Stopwatch.StartNew();
                XpManager.Init();
                NewsService.Init();
                ConsoleHelper.Log($"Finished initializing services! ({sw.ElapsedMilliseconds}ms)", LogSeverity.Info);

                Thread.Sleep(-1);
            } catch (Exception ex)
            {
                string nl = Environment.NewLine;
                File.WriteAllText(Path.Combine(Information.Directory, "error.txt"),
                    $"[{DateTime.Now:dd.MM.yyyy hh:mm:ss}] {ex.Message}:{nl}{ex.Message}{nl}{ex.Source}{nl}{ex.StackTrace}");
            }
        }
    }
}