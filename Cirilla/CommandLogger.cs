﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cirilla.Services.Pastebin;
using Discord;

namespace Cirilla
{
    public static class CommandLogger
    {
        private static object Lock { get; } = new object();


        public static void Log(string username, string guildname, string command)
        {
            new Thread(() =>
            {
                lock (Lock)
                {
                    try
                    {
                        string logfile = Path.Combine(Information.Directory, "commandlog.txt");
                        if (!File.Exists(logfile)) File.Create(logfile).Dispose();

                        // > than 10 MB (default) -> clear log
                        if (Information.Config != null && new FileInfo(logfile).Length > Information.MaxLogSize)
                            File.WriteAllBytes(logfile, new byte[0]);

                        File.AppendAllLines(logfile,
                            new List<string> { $"[{DateTime.Now:HH:mm:ss}] [{username}@{guildname}] \"{command}\"" });
                    } catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Could not save Log to Command Log File! ({ex.Message})");
                        Console.ResetColor();
                    }
                }
            }).Start();
        }


        public static async Task Upload(IDMChannel dm, IMessageChannel channel)
        {
            IMessage message = await channel.SendMessageAsync("One sec..");

            string file = Path.Combine(Information.Directory, "commandlog.txt");
            string log = File.ReadAllText(file);
            //upload to pastebin
            string link = await Pastebin.Post(log);

            await dm.SendMessageAsync($"Here you go <{link}>");

            await message.DeleteAsync();
        }


        public static long Clear()
        {
            lock (Lock)
            {
                try
                {
                    string logfile = Path.Combine(Information.Directory, "commandlog.txt");
                    if (!File.Exists(logfile)) File.Create(logfile).Dispose();
                    long size = new FileInfo(logfile).Length / 1024;
                    File.WriteAllBytes(logfile, new byte[0]);
                    return size;
                } catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Could not clear Command Log File! ({ex.Message})");
                    Console.ResetColor();
                }

                return -1;
            }
        }
    }
}