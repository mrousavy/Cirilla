﻿using System;
using System.Threading.Tasks;
using Cirilla.Services.GuildConfig;
using Discord;
using Discord.Commands;

namespace Cirilla.Modules
{
    public class Admin : ModuleBase
    {
        [Command("nick")]
        [Summary("Change Bot's nickname")]
        public async Task SetNickname([Summary("The new nickname")] string nickname)
        {
            try
            {
                if (!(Context.User is IGuildUser user)) return;
                if (!Helper.IsOwner(user) && !user.GuildPermissions.Administrator)
                {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                if (!(await Context.Guild.GetCurrentUserAsync()).GuildPermissions.ChangeNickname)
                {
                    await ReplyAsync("I can't change my nickname here.. :confused:");
                    return;
                }

                var bot = await Context.Guild.GetCurrentUserAsync();
                await bot.ModifyAsync(p => p.Nickname = nickname);
                await ReplyAsync("Nickname changed!");
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't set the nickname.. :confused:");
                ConsoleHelper.Log($"Error setting nickname, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("setup")]
        [Summary("Setup the bot!")]
        public async Task SetupBot()
        {
            try
            {
                if (!(Context.User is IGuildUser user)) return;
                if (!Helper.IsOwner(user) && !user.GuildPermissions.Administrator)
                {
                    await ReplyAsync("Sorry, only admins can use this command!");
                    return;
                }

                var me = await Context.Guild.GetCurrentUserAsync();
                if (me.GuildPermissions.ManageChannels)
                {
                    var botchat = await Context.Guild.CreateTextChannelAsync(Information.Botchat);
                    await botchat.SendMessageAsync("Hi guys! You can chat with me here! :smile:");
                } else
                {
                    await ReplyAsync(
                        $"If you guys want to talk to me, you gotta create #{Information.Botchat}.. Couldn't do it myself!");
                }

                await ReplyAsync("I'm ready to go! :smile:");
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't setup myself.. :confused:");
                ConsoleHelper.Log($"Error setting up bot on \"{Context.Guild.Name}\", {ex.Message}!",
                    LogSeverity.Error);
            }
        }

        [Command("leave")]
        [Summary("Leave this Guild")]
        public async Task LeaveGuild()
        {
            try
            {
                if (!(Context.User is IGuildUser user)) return;
                if (!Helper.IsOwner(user) && !user.GuildPermissions.Administrator)
                {
                    await ReplyAsync("Sorry, only admins can use this command!");
                    return;
                }

                await ReplyAsync($"Bye guys, {Helper.GetName(user)} wanted me to leave! :disappointed_relieved:");
                ConsoleHelper.Log($"{Context.User} requested the bot to leave \"{Context.Guild.Name}\"!",
                    LogSeverity.Info);

                await Context.Guild.LeaveAsync();
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't leave this guild.. :confused:");
                ConsoleHelper.Log($"Error leaving guild, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("togglexp")]
        [Summary("Enable or disable the XP System for this Guild")]
        public async Task ToggleXp()
        {
            try
            {
                if (!(Context.User is IGuildUser user)) return;
                if (!Helper.IsOwner(user) && !user.GuildPermissions.Administrator)
                {
                    await ReplyAsync("Sorry, only admins can use this command!");
                    return;
                }

                var config = GuildConfigManager.Get(Context.Guild.Id);
                bool after = !config.EnableXpSystem;

                config.EnableXpSystem = after;
                GuildConfigManager.Set(config);

                if (after)
                {
                    await ReplyAsync("XP System for this Guild is now enabled!");
                    ConsoleHelper.Log($"{Context.User} enabled XP System for Guild \"{Context.Guild.Name}\"!",
                        LogSeverity.Info);
                } else
                {
                    //string guildDir = Path.Combine(Information.Directory, Context.Guild.Id.ToString());
                    ////cleanup
                    //if (Directory.Exists(guildDir)) {
                    //    Directory.Delete(guildDir, true);
                    //}
                    await ReplyAsync("XP System for this Guild is now disabled!");
                    ConsoleHelper.Log($"{Context.User} disabled XP System for Guild \"{Context.Guild.Name}\"!",
                        LogSeverity.Info);
                }
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't toggle the XP System.. :confused:");
                ConsoleHelper.Log($"Error toggling XP system for guild, {ex.Message}!", LogSeverity.Error);
            }
        }


        [Command("prefix")]
        [Summary("Change prefix")]
        public async Task ChangePrefix([Summary("New prefix")] [Remainder] string prefix)
        {
            try
            {
                if (!(Context.User is IGuildUser user)) return;

                if (!Helper.IsOwner(user) && !user.GuildPermissions.Administrator)
                {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                var config = GuildConfigManager.Get(Context.Guild.Id);
                string before = config.Prefix;
                bool disablePrimary = config.EnablePrimaryPrefix;

                GuildConfigManager.Set(Context.Guild.Id, prefix, disablePrimary);
                await ReplyAsync($"Prefix changed from `{before}` to `{prefix}`!");
                ConsoleHelper.Log(
                    $"{Context.User} changed the prefix on \"{Context.Guild.Name}\" from {before} to {prefix}!",
                    LogSeverity.Info);
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't change the prefix.. :confused:");
                ConsoleHelper.Log($"Error chaning prefix, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("prefix")]
        [Summary("Get prefixes")]
        public async Task GetPrefix()
        {
            if (!(Context.User is IGuildUser))
            {
                await ReplyAsync("Those are my global prefixes: " +
                                 $"`{Information.SecondaryPrefix}`, `{Information.Prefix}`, {Cirilla.Client.CurrentUser.Mention}");
                return;
            }

            var config = GuildConfigManager.Get(Context.Guild.Id);
            string prefixes = string.Empty;
            if (config.EnablePrimaryPrefix)
                prefixes += $"`{Information.Prefix}` ";
            prefixes += $"`{config.Prefix}`, {Cirilla.Client.CurrentUser.Mention}";

            await ReplyAsync($"Those are my current prefixes: {prefixes}");
        }


        [Command("toggleprimary")]
        [Summary("Enable or disable the primary Prefix ($)")]
        public async Task TogglePrimaryPrefix()
        {
            try
            {
                if (!(Context.User is IGuildUser user)) return;

                if (!Helper.IsOwner(user) && !user.GuildPermissions.Administrator)
                {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                var config = GuildConfigManager.Get(Context.Guild.Id);
                bool before = config.EnablePrimaryPrefix;
                bool after = !config.EnablePrimaryPrefix;
                config.EnablePrimaryPrefix = after;
                GuildConfigManager.Set(config.GuildId, config.Prefix, config.EnablePrimaryPrefix);

                if (after) await ReplyAsync("Bot is now also listening to primary prefix (`$`)!");
                else
                    await ReplyAsync("Bot is not listening to primary prefix anymore (`$`)!" + Environment.NewLine +
                                     $"Current prefixes: `{config.Prefix}` and \"{Cirilla.Client.CurrentUser.Mention}\"");

                ConsoleHelper.Log($"{Context.User} toggled the primary prefix on \"{Context.Guild.Name}\" from " +
                                  $"{before} to {after}!",
                    LogSeverity.Info);
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't toggle the primary prefix.. :confused:");
                ConsoleHelper.Log($"Error toggling primary prefix, {ex.Message}!", LogSeverity.Error);
            }
        }
    }
}