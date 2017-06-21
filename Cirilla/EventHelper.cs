using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cirilla {
    public static class EventHelper {
        public static async Task UserJoined(SocketGuildUser arg) {
            await ConsoleHelper.Log($"{Helper.GetName(arg)} joined the \"{arg.Guild.Name}\" server!",
                Discord.LogSeverity.Info);
            SocketTextChannel channel =
                (new List<SocketTextChannel>(arg.Guild.TextChannels)).FirstOrDefault(
                    c => c.Name == Information.TextChannel);
            if (channel != null)
                await channel.SendMessageAsync($"Hi {arg.Mention}!", true);
        }

        public static async Task UserLeft(SocketGuildUser arg) {
            await ConsoleHelper.Log($"{arg.Username} left the \"{arg.Guild.Name}\" server!", Discord.LogSeverity.Info);
            SocketTextChannel channel =
                (new List<SocketTextChannel>(arg.Guild.TextChannels)).FirstOrDefault(
                    c => c.Name == Information.TextChannel);
            if (channel != null)
                await channel.SendMessageAsync($"{Helper.GetName(arg)} left the server!", true);

            RestDMChannel dm = await arg.CreateDMChannelAsync();
            await dm.SendMessageAsync("Why did you leave man?", true);
        }


        public static async Task JoinedGuild(SocketGuild arg) {
            try {
                if (arg.CurrentUser.GuildPermissions.SendMessages) {
                    await arg.DefaultChannel.SendMessageAsync("Hi guys! I'm the new bot!! :wave: :smile:");

                    string dataPath = Path.Combine(Information.Directory, arg.Id.ToString());
                    if (!Directory.Exists(dataPath)) {
                        Directory.CreateDirectory(dataPath);
                    }
                }
            } catch (Exception ex) {
                await ConsoleHelper.Log($"An unknown error occured (JoinedGuild event) {ex.Message}",
                    Discord.LogSeverity.Error);
            }
        }

        public static Task LeftGuild(SocketGuild arg) {
            try {
                string guildDir = Path.Combine(Information.Directory, arg.Id.ToString());
                //cleanup
                if (Directory.Exists(guildDir)) {
                    Directory.Delete(guildDir, true);
                }
            } catch (Exception ex) {
                ConsoleHelper.Log($"Could not cleanup for \"{arg.Name}\" guild! ({ex.Message})",
                    Discord.LogSeverity.Error);
            }
            ConsoleHelper.Log($"Left \"{arg.Name}\" guild!", Discord.LogSeverity.Info);

            return Task.CompletedTask;
        }
    }
}
