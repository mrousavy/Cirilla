using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cirilla {
    public static class CommandLogger {
        private static object Lock { get; set; } = new object();


        public static void Log(string username, string guildname, string command) {
            new Thread(() => {
                lock (Lock) {
                    try {
                        string logfile = Path.Combine(Information.Directory, "commandlog.txt");
                        if (!File.Exists(logfile)) {
                            File.Create(logfile).Dispose();
                        }

                        // > than 10 MB (default) -> clear log
                        if (Information.Config != null && new FileInfo(logfile).Length > Information.MaxLogSize) {
                            File.WriteAllBytes(logfile, new byte[0]);
                        }

                        File.AppendAllLines(logfile, new List<string> { $"{DateTime.Now:HH:mm:ss}: {username}@{guildname}: {command}" });
                    } catch (Exception ex) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Could not save Log to Command Log File! ({ex.Message})");
                        Console.ResetColor();
                    }
                }
            }).Start();
        }


        public static async Task Upload(IDMChannel dm) {
            string file = Path.Combine(Information.Directory, "commandlog.txt");
            string filecopy = Path.Combine(Information.Directory, "commandlog_copy.txt");

            lock (Lock) {
                File.Copy(file, filecopy);
            }
            await dm.SendFileAsync(filecopy, "Here you go");
            lock (Lock) {
                File.Delete(filecopy);
            }
        }
    }
}
