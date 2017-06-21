using Cirilla.Services.Reminder;
using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Reminder : ModuleBase {
        [Command("remindme"), Summary("Remind !")]
        public async Task RemindMe(
            [Summary("The time until I should remind you, format: yyyy:MM:dd:HH:mm:ss")] TimeSpan time,
            [Summary("The text you want to get reminded of")][Remainder]string text) {
            try {
                IGuildUser user = Context.User as IGuildUser;
                if (user == null) {
                    return;
                }
                int maxDays = Information.MaximumRemindDays;
                if (time.Days > maxDays) {
                    await ReplyAsync($"Sorry I'm not allowing you to set a reminder for anything longer than {maxDays} days..");
                    return;
                }

                await ReminderService.AddReminder(Context.User.Mention, text, DateTime.Now + time, Context.Guild);
                await ConsoleHelper.Log($"{Helper.GetName(Context.User)} set a reminder for {(DateTime.Now + time):dd.MM.yyyy HH:mm}!", LogSeverity.Info);
                await ReplyAsync($"All set {Helper.GetName(Context.User)}, I'll remind you at {(DateTime.Now + time):dd.MM.yyyy HH:mm}!");
            } catch (MaximumRemindersException) {
                await ReplyAsync($"You can't have more than {Information.MaximumReminders} reminders simultaneously!");
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't set that reminder.. :confused:");
                await ConsoleHelper.Log($"Error setting reminder for {Helper.GetName(Context.User)} at " +
                    $"{(DateTime.Now + time):yyyyMMddHHmm}! ({ex.Message})", LogSeverity.Error);
            }
        }
    }
}
