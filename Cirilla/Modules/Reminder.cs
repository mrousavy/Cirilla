using System;
using System.Threading.Tasks;
using Cirilla.Services.Reminder;
using Discord;
using Discord.Commands;

namespace Cirilla.Modules
{
    public class Reminder : ModuleBase
    {
        [Command("remindme")]
        [Summary("Set yourself a reminder!")]
        public async Task RemindMe(
            [Summary("The time until I should remind you, format: 0d:0h:0m:0s")]
            Timediff time,
            [Summary("The text you want to get reminded of")] [Remainder]
            string text)
        {
            try
            {
                var user = Context.User as IGuildUser;
                if (user == null)
                {
                    await ReplyAsync("You can't set reminders in DM!");
                    return;
                }

                int maxDays = Information.MaximumRemindDays;
                if (time.Days > maxDays)
                {
                    await ReplyAsync(
                        $"Sorry I'm not allowing you to set a reminder for anything longer than {maxDays} days..");
                    return;
                }

                await ReminderService.AddReminder(Context.User, text, DateTime.Now + time.Span, Context.Guild);
                ConsoleHelper.Log(
                    $"{Helper.GetName(Context.User)} set a reminder for {DateTime.Now + time.Span:dd.MM.yyyy HH:mm}!",
                    LogSeverity.Info);

                //Don't say Day when it's the same day
                string when;
                if ((DateTime.Now + time.Span).DayOfYear == DateTime.Now.DayOfYear)
                    when = (DateTime.Now + time.Span).ToString("HH:mm");
                else when = (DateTime.Now + time.Span).ToString("dd.MM.yyyy HH:mm");

                await ReplyAsync(
                    $"All set {Helper.GetName(Context.User)}, I'll remind you at **{when}**!");
            } catch (MaximumRemindersException)
            {
                await ReplyAsync($"You can't have more than {Information.MaximumReminders} reminders simultaneously!");
            } catch (Exception ex)
            {
                await ReplyAsync("Whoops, unfortunately I couldn't set that reminder.. :confused:");
                ConsoleHelper.Log($"Error setting reminder for {Helper.GetName(Context.User)} at " +
                                  $"{DateTime.Now + time.Span:yyyyMMddHHmm}! ({ex.Message})", LogSeverity.Error);
            }
        }
    }
}