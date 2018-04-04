using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cirilla.Services.GuildConfig;
using Discord;
using Discord.Commands;

namespace Cirilla.Modules
{
    public class BotInfo : ModuleBase
    {
        [Command("host")]
        [Summary("Shows host information")]
        public async Task Host()
        {
            try
            {
                string mname = Environment.MachineName;

                string pre = string.Empty;
                bool enablePrimary = true;
                string sPrefix = Information.SecondaryPrefix;
                if (Context.Guild == null)
                {
                    // It's DM
                } else
                {
                    var config = GuildConfigManager.Get(Context.Guild.Id);
                    enablePrimary = config.EnablePrimaryPrefix;
                    sPrefix = config.Prefix;
                }

                if (enablePrimary) pre += $"`{Information.Prefix}`, ";
                pre += $"`{sPrefix}`, {Context.Client.CurrentUser.Mention}";

                int cores = Environment.ProcessorCount;
                var current = Process.GetCurrentProcess();

                var builder = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "Bot Information",
                        IconUrl = Information.BotIconUrl
                    },
                    Color = new Color(50, 125, 0),
                    ThumbnailUrl = Information.IconUrl
                };
                builder.AddInlineField("Uptime", GetUptime());
                builder.AddInlineField("Machine", mname);
                builder.AddInlineField("Core #", $"{cores} cores");
                builder.AddInlineField("RAM usage",
                    $"{(double) current.WorkingSet64 / 1024 / 1024:#.#} / {(double) current.PeakWorkingSet64 / 1024 / 1024:#.#} MB");
                builder.AddInlineField("Prefixes", pre);
                builder.AddInlineField("Source Code", $"[GitHub]({Information.RepoUrl})");
                builder.AddInlineField("My Senpai", Information.Owner);
                builder.AddInlineField("Guilds", $"Online on {Cirilla.Client.Guilds.Count} guilds");

                await ReplyAsync("", embed: builder.Build());
                ConsoleHelper.Log($"{Context.User} requested Bot/Host Information!", LogSeverity.Info);
            } catch (Exception ex)
            {
                await ReplyAsync("Sorry, I can't send you that information right now!");
                ConsoleHelper.Log($"Error retrieving Host information {ex.Message}", LogSeverity.Error);
            }
        }

        [Command("owner")]
        [Summary("Show Cirilla bot owner")]
        public async Task Owner()
        {
            await ReplyAsync($"My owner is \"mrousavy#6472\"; ({Information.Owner}) - http://github.com/mrousavy");
        }

        [Command("invite")]
        [Summary("Show Guild join link for bot")]
        public async Task Invite()
        {
            await ReplyAsync(Information.InviteLink);
        }

        [Command("uptime")]
        [Summary("Shows bot uptime")]
        public async Task Uptime()
        {
            try
            {
                var tspan = DateTime.Now - Program.StartTime;
                if (tspan.TotalDays >= 1)
                    await ReplyAsync(
                        $"I'm already running for {tspan:d'd 'h'h m'm 's's'}, I'm tired.. :confused:");
                else if (tspan.TotalHours >= 1)
                    await ReplyAsync(
                        $"I'm already running for {tspan:h'h 'm'm 's's'}.");
                else if (tspan.TotalMinutes >= 1) await ReplyAsync($"I'm running for {tspan:m'm 's's'}.");
                else await ReplyAsync($"I'm running for {tspan:s's'}.");
                ConsoleHelper.Log($"{Context.User} requested Uptime!", LogSeverity.Info);
            } catch (Exception ex)
            {
                await ReplyAsync("Sorry, I can't send you that information right now!");
                ConsoleHelper.Log($"Error retrieving uptime information {ex.Message}", LogSeverity.Error);
            }
        }

        private static string GetUptime()
        {
            var tspan = DateTime.Now - Program.StartTime;
            string uptime;
            if (tspan.TotalDays >= 1) uptime = tspan.ToString("d'd 'h'h 'm'm 's's'");
            else if (tspan.TotalHours >= 1) uptime = tspan.ToString("h'h 'm'm 's's'");
            else if (tspan.TotalMinutes >= 1) uptime = tspan.ToString("m'm 's's'");
            else uptime = tspan.ToString("s's'");
            return uptime;
        }


        [Command("run")]
        [Summary("Shows how to run this bot on your own")]
        public async Task Run()
        {
            try
            {
                var builder = new EmbedBuilder
                {
                    Title = "How to run Cirilla Bot:",
                    Color = new Color(50, 125, 0)
                };
                builder.AddField("1. Clone", $"Clone project from GitHub (git bash: `git clone {Information.RepoUrl}`");
                builder.AddField("2. Open Bash/CLI",
                    "Run bash or any other command line tool and navigate into the Project folder: " +
                    "`cd C:\\Some\\Directory\\Cirilla\\Cirilla`");
                builder.AddField("4. DNX Restore", "Run `dotnet restore` in the Cirilla Project Folder");
                builder.AddField("5. DNX Run",
                    "Run `dotnet run` (Requires [.NET Core Tools](https://www.microsoft.com/net/download/core#/runtime))");

                await ReplyAsync("", embed: builder.Build());
            } catch
            {
                // channel not available anymore? any unexpected error
            }
        }

        [Command("source")]
        [Summary("Shows source code of Cirilla")]
        public async Task Source()
        {
            await ReplyAsync($"I'm written in C#.NET Core, my source code is on GitHub; {Information.RepoUrl}");
        }
    }
}