using Cirilla.Services.Reminder;
using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Reminder : ModuleBase {
        [Command("remindme"), Summary("Remind !")]
        public async Task RemindMe(
            [Summary("The amount of time you want to be reminded")] TimeSpan time,
            [Summary("The text you want to get reminded of")][Remainder]string text) {
            try {
                IGuildUser user = Context.User as IGuildUser;
                if (user == null) {
                    return;
                }

                ReminderService.AddReminder(Context.User.Mention, text, DateTime.Now + time, Context.Guild);
                await ConsoleHelper.Log($"All set {Helper.GetName(Context.User)}, I'll remind you at {(DateTime.Now + time):yyyyMMddHHmm}!", LogSeverity.Error);
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't set that reminder.. :confused:");
                await ConsoleHelper.Log($"Error setting reminder for {Helper.GetName(Context.User)} at " +
                    $"{(DateTime.Now + time):yyyyMMddHHmm}! ({ex.Message})", LogSeverity.Error);
            }
        }
    }
}
