using Cirilla.Services.Reminder;
using Cirilla.Services.Xp;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Cirilla {
    public static class EventHelper {
        public static async Task UserJoined(SocketGuildUser user) {
            await ConsoleHelper.Log($"{Helper.GetName(user)} joined the \"{user.Guild.Name}\" server!",
                Discord.LogSeverity.Info);
            SocketTextChannel channel = user.Guild.DefaultChannel;
            if (channel != null)
                await channel.SendMessageAsync($"Hi {user.Mention}!", true);
        }

        public static async Task UserLeft(SocketGuildUser user) {
            await ConsoleHelper.Log($"{user.Username} left the \"{user.Guild.Name}\" server!", Discord.LogSeverity.Info);
            SocketTextChannel channel = user.Guild.DefaultChannel;
            if (channel != null)
                await channel.SendMessageAsync($"{Helper.GetName(user)} left the server!", true);

            XpManager.RemoveUser(user.Guild, user);

            RestDMChannel dm = await user.CreateDMChannelAsync();
            await dm.SendMessageAsync("Why did you leave man?", true);
        }


        public static async Task JoinedGuild(SocketGuild guild) {
            try {
                if (guild.CurrentUser.GuildPermissions.SendMessages) {
                    await guild.DefaultChannel.SendMessageAsync($"Hi guys! I'm the new bot!! :wave: :smile:" +
                        $"{Environment.NewLine}Type `{Information.Prefix}help` for a list of all available commands!");

                    string dataPath = Path.Combine(Information.Directory, guild.Id.ToString());
                    if (!Directory.Exists(dataPath)) {
                        Directory.CreateDirectory(dataPath);
                    }
                }
            } catch (Exception ex) {
                await ConsoleHelper.Log($"An unknown error occured (JoinedGuild event) {ex.Message}",
                    Discord.LogSeverity.Error);
            }
        }

        public static Task LeftGuild(SocketGuild guild) {
            try {
                string guildDir = Path.Combine(Information.Directory, guild.Id.ToString());
                //cleanup
                if (Directory.Exists(guildDir)) {
                    Directory.Delete(guildDir, true);
                }
            } catch (Exception ex) {
                ConsoleHelper.Log($"Could not cleanup for \"{guild.Name}\" guild! ({ex.Message})",
                    Discord.LogSeverity.Error);
            }
            ConsoleHelper.Log($"Left \"{guild.Name}\" guild!", Discord.LogSeverity.Info);

            return Task.CompletedTask;
        }


        public static Task GuildAvailable(SocketGuild guild) {
            try {
                ReminderService.Init(guild);
            } catch (Exception ex) {
                ConsoleHelper.Log($"Could not init Guild's reminders! ({ex.Message})", Discord.LogSeverity.Error);
            }
            return Task.CompletedTask;
        }
    }
}
