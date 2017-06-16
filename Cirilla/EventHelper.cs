﻿using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;
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
    }
}