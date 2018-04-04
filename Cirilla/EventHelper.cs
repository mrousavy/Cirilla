using System;
using System.IO;
using System.Threading.Tasks;
using Cirilla.Modules;
using Cirilla.Services.Reminder;
using Cirilla.Services.Xp;
using Discord;
using Discord.WebSocket;

namespace Cirilla
{
    public static class EventHelper
    {
        public static async Task UserJoined(SocketGuildUser user)
        {
            ConsoleHelper.Log($"{Helper.GetName(user)} joined the \"{user.Guild.Name}\" server!",
                LogSeverity.Info);
            var channel = user.Guild.DefaultChannel;
            if (channel != null)
                await channel.SendMessageAsync($"Hi {user.Mention}!", true);
        }

        public static async Task UserLeft(SocketGuildUser user)
        {
            ConsoleHelper.Log($"{user} left the \"{user.Guild.Name}\" server!", LogSeverity.Info);
            var channel = user.Guild.DefaultChannel;
            if (channel != null)
                await channel.SendMessageAsync($"{Helper.GetName(user)} left the server!", true);

            XpManager.RemoveUser(user.Guild, user);

            var dm = await user.GetOrCreateDMChannelAsync();
            await dm.SendMessageAsync("Why did you leave man?", true);
        }


        public static async Task JoinedGuild(SocketGuild guild)
        {
            try
            {
                ConsoleHelper.Log($"Joined Guild \"{guild.Name}\"!", LogSeverity.Info);

                if (guild.CurrentUser.GuildPermissions.SendMessages)
                {
                    await guild.DefaultChannel.SendMessageAsync("Hi guys! I'm the new bot!! :wave: :smile:" +
                                                                $"{Environment.NewLine}Type `{Information.Prefix}help` for a list of all available commands!");

                    string dataPath = Path.Combine(Information.Directory, guild.Id.ToString());
                    if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
                }

                await Cirilla.Client.SetGameAsync($"{Information.Prefix}help | {Cirilla.Client.Guilds.Count} Guilds");
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"An unknown error occured (JoinedGuild event) {ex.Message}",
                    LogSeverity.Error);
            }
        }

        public static Task LeftGuild(SocketGuild guild)
        {
            try
            {
                string guildDir = Path.Combine(Information.Directory, guild.Id.ToString());
                //cleanup
                if (Directory.Exists(guildDir)) Directory.Delete(guildDir, true);
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Could not cleanup for \"{guild.Name}\" guild! ({ex.Message})",
                    LogSeverity.Error);
            }

            ConsoleHelper.Log($"Left Guild \"{guild.Name}\"!", LogSeverity.Info);

            return Task.CompletedTask;
        }


        public static Task GuildAvailable(SocketGuild guild)
        {
            try
            {
                ReminderService.Init(guild);
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Could not init Guild's reminders! ({ex.Message})", LogSeverity.Error);
            }

            return Task.CompletedTask;
        }

        public static async Task Ready()
        {
            await Cirilla.Client.SetGameAsync($"{Information.Prefix}help | {Cirilla.Client.Guilds.Count} Guilds");


            ConsoleHelper.Log("Connected Guilds:", LogSeverity.Info);
            foreach (IGuild guild in Cirilla.Client.Guilds)
                ConsoleHelper.Log($"\t{guild.Name}", LogSeverity.Info);
        }


        public static async Task QuestionThinking(SocketMessage messageArg)
        {
            if (!(messageArg is SocketUserMessage message))
                return;

            try
            {
                if (message.Content.EndsWith("?")) await message.AddReactionAsync(new Emoji(Information.ThinkEmoji));
            } catch
            {
                // can't add reactions
            }
        }

        public static async Task RandomEmoji(SocketMessage messageArg)
        {
            if (!(messageArg is SocketUserMessage message))
                return;

            if (message.Channel is IGuildChannel guildchannel)
            {
                var emote = RandomEmote.GetRandomEmote(guildchannel.Guild);
                if (emote != null)
                    try
                    {
                        await message.AddReactionAsync(emote);
                        ConsoleHelper.Log("Added random Emote to a message!", LogSeverity.Info);
                    } catch
                    {
                        // can't add reactions
                    }
            }
        }
    }
}