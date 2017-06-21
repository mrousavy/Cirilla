using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cirilla.Services.Reminder {
    public class ReminderService {
        public static async void AddReminder(string userMention, string text, DateTime time, IGuild guild) {
            Reminders.Add(new UserReminder(userMention, text, time));
            WriteOut(guild.Id);

            ITextChannel channel = await guild.GetDefaultChannelAsync();
            ReminderWaiter(new UserReminder(userMention, text, time), channel);
        }


        private static void WriteOut(ulong guildId) {
            string path = Path.Combine(Information.Directory, guildId.ToString(), "reminder.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(ReminderListJson));
        }

        public static void Init(SocketGuild guild) {
            string path = Path.Combine(Information.Directory, guild.Id.ToString(), "reminder.json");

            if (File.Exists(path)) {
                try {
                    string serialized = File.ReadAllText(path);
                    ReminderListJson = JsonConvert.DeserializeObject<ReminderListJson>(serialized);
                } catch (Exception ex) {
                    ConsoleHelper.Log($"Could not load reminder! ({ex.Message})", LogSeverity.Info);
                }
            } else {
                File.WriteAllText(path, JsonConvert.SerializeObject(ReminderListJson));
            }

            foreach (UserReminder reminder in Reminders) {
                new Thread(() => {
                    ITextChannel channel = guild.DefaultChannel;
                    ReminderWaiter(reminder, channel);
                });
            }
        }

        private static async void ReminderWaiter(UserReminder reminder, ITextChannel channel) {
            TimeSpan offset;
            if (DateTime.Now > reminder.Time) {
                offset = DateTime.Now - reminder.Time;
            } else {
                offset = reminder.Time - DateTime.Now;
            }

            await Task.Delay((int)offset.TotalMilliseconds);

            string message = $"{reminder.UserMention}, you told me to remind you to \"{reminder.Text}\"!";
            await channel.SendMessageAsync(message);
        }

        public static List<UserReminder> Reminders => ReminderListJson.Reminders;

        public static ReminderListJson ReminderListJson = new ReminderListJson();
    }

    public class ReminderListJson {
        public List<UserReminder> Reminders = new List<UserReminder>();
    }

    public class UserReminder {
        public UserReminder(string userMention, string text, DateTime time) {
            UserMention = userMention;
            Text = text;
            Time = time;
        }

        public string UserMention { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
    }
}
