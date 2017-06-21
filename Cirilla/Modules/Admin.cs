using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Admin : ModuleBase {
        [Command("nick"), Summary("Change Bot's nickname")]
        public async Task SetNickname([Summary("The new nickname")] string nickname) {
            try {
                IGuildUser user = Context.User as IGuildUser;
                if (user == null) {
                    return;
                }
                if (!user.GuildPermissions.Has(GuildPermission.Administrator)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }
                if (!(await Context.Guild.GetCurrentUserAsync()).GuildPermissions.ChangeNickname) {
                    await ReplyAsync("I can't change my nickname here.. :confused:");
                    return;
                }

                IGuildUser bot = await Context.Guild.GetCurrentUserAsync();
                await bot.ModifyAsync(p => p.Nickname = nickname);
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't set the nickname.. :confused:");
                await ConsoleHelper.Log($"Error setting nickname, {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("setup"), Summary("Setup the bot!")]
        public async Task SetupBot() {
            try {
                IGuildUser user = Context.User as IGuildUser;
                if (user == null) {
                    return;
                }
                if (!user.GuildPermissions.Administrator) {
                    await ReplyAsync("Sorry, only admins can use this command!");
                    return;
                }

                IGuildUser me = await Context.Guild.GetCurrentUserAsync();
                if (me.GuildPermissions.ManageChannels) {
                    ITextChannel botchat = await Context.Guild.CreateTextChannelAsync(Information.Botchat);
                    await botchat.SendMessageAsync("Hi guys! You can chat with me here! :smile:");
                } else {
                    await ReplyAsync($"If you guys want to talk to me, you gotta create #{Information.Botchat}.. Couldn't do it myself!");
                }

                await ReplyAsync("I'm ready to go! :smile:");
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't setup myself.. :confused:");
                await ConsoleHelper.Log($"Error setting up bot on \"{Context.Guild.Name}\", {ex.Message}!", LogSeverity.Error);
            }
        }

        [Command("leave"), Summary("Leave this Guild")]
        public async Task LeaveGuild() {
            try {
                IGuildUser user = Context.User as IGuildUser;
                if (user == null) {
                    return;
                }
                if (!user.GuildPermissions.Administrator) {
                    await ReplyAsync("Sorry, only admins can use this command!");
                    return;
                }

                await ReplyAsync($"Bye guys, {Helper.GetName(user)} wanted me to leave! :disappointed_relieved:");
                await ConsoleHelper.Log($"{Context.User} requested the bot to leave \"{Context.Guild.Name}\"!",
                    LogSeverity.Info);

                await Context.Guild.LeaveAsync();
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't leave this guild.. :confused:");
                await ConsoleHelper.Log($"Error leaving guild, {ex.Message}!", LogSeverity.Error);
            }
        }
    }
}