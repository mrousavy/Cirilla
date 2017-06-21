using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cirilla.Services.Reminder {
    public class ReminderService {
        public static object Lock { get; set; } = new object();

        public static async Task AddReminder(string userMention, string text, DateTime time, IGuild guild) {
            if (Reminders.Where(ur => ur.UserMention == userMention).Count() + 1 > Information.MaximumReminders) {
                throw new MaximumRemindersException($"Maximum simultaneous reminders for {userMention} has been reached!");
            }

            UserReminder reminder = new UserReminder(userMention, text, time);
            Reminders.Add(reminder);
            WriteOut(guild);

            ITextChannel channel = await guild.GetDefaultChannelAsync();
            Task task = ReminderWaiter(reminder, channel, guild);
        }


        private static void WriteOut(IGuild guild) {
            string path = Helper.GetPath(guild, "reminder.json");
            lock (Lock) {
                File.WriteAllText(path, JsonConvert.SerializeObject(ReminderListJson));
            }
        }

        public static void Init(SocketGuild guild) {
            string path = Helper.GetPath(guild, "reminder.json");


            lock (Lock) {
                if (File.Exists(path)) {
                    try {
                        string serialized = File.ReadAllText(path);
                        ReminderListJson = JsonConvert.DeserializeObject<ReminderListJson>(serialized);
                    } catch (Exception ex) {
                        ConsoleHelper.Log($"Could not load reminder! ({ex.Message})", LogSeverity.Info);
                    }
                } else {
                    File.Create(path).Dispose();
                    File.WriteAllText(path, JsonConvert.SerializeObject(ReminderListJson));
                }
            }

            if (Reminders == null) {
                ReminderListJson = new ReminderListJson();
            }
            foreach (UserReminder reminder in Reminders) {
                new Thread(() => {
                    ITextChannel channel = guild.DefaultChannel;
                    ReminderWaiter(reminder, channel, guild).Wait();
                }).Start();
            }
        }

        private static async Task ReminderWaiter(UserReminder reminder, ITextChannel channel, IGuild guild) {
            TimeSpan offset = reminder.Time - DateTime.Now;

            //Remove if already passed
            if (offset.TotalMilliseconds < 0) {
                for (int i = 0; i < Reminders.Count; i++) {
                    if (Reminders[i].Equals(reminder)) {
                        Reminders.RemoveAt(i);
                        i--;
                    }
                }
                return;
            }

            await Task.Delay((int)offset.TotalMilliseconds);

            string message = $"{reminder.UserMention}, you told me to remind you to: \"_{reminder.Text}_\"!";
            await channel.SendMessageAsync(message);

            for (int i = 0; i < Reminders.Count; i++) {
                if (Reminders[i].Equals(reminder)) {
                    Reminders.RemoveAt(i);
                    i--;
                }
            }
            WriteOut(guild);
        }

        public static List<UserReminder> Reminders {
            get {
                if (ReminderListJson == null) {
                    return null;
                }
                return ReminderListJson.Reminders;
            }
        }

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

        public override bool Equals(object obj) {
            if (obj is UserReminder reminder2) {
                return
                    UserMention == reminder2.UserMention &&
                    Text == reminder2.Text &&
                    Time == reminder2.Time;
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    public class MaximumRemindersException : Exception {
        public MaximumRemindersException(string message) : base(message) { }
    }
}
