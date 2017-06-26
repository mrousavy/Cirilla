using Cirilla.Services.Reminder;
using Cirilla.Services.Xp;
using Discord;
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
            await ConsoleHelper.Log($"{user} left the \"{user.Guild.Name}\" server!", Discord.LogSeverity.Info);
            SocketTextChannel channel = user.Guild.DefaultChannel;
            if (channel != null)
                await channel.SendMessageAsync($"{user.Mention} left the server!", true);

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

                await Cirilla.Client.SetGameAsync($"{Information.Prefix}help | {Cirilla.Client.Guilds.Count}");
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


        public static async Task GuildAvailable(SocketGuild guild) {
            try {
                ReminderService.Init(guild);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not init Guild's reminders! ({ex.Message})", LogSeverity.Error);
            }
        }

        public static async Task Ready() {
            await Cirilla.Client.SetGameAsync($"{Information.Prefix}help | {Cirilla.Client.Guilds.Count}");
        }



        public static async Task QuestionThinking(SocketMessage messageArg) {
            SocketUserMessage message = messageArg as SocketUserMessage;
            if (message == null)
                return;

            try {
                if (message.Content.EndsWith("?")) {
                    await message.AddReactionAsync(new Emoji(Information.ThinkEmoji));
                }
            } catch {
                // can't add reactions
            }
        }
        public static async Task RandomEmoji(SocketMessage messageArg) {
            SocketUserMessage message = messageArg as SocketUserMessage;
            if (message == null)
                return;

            if (message.Channel is IGuildChannel guildchannel) {
                IEmote emote = Modules.RandomEmote.GetRandomEmote(guildchannel.Guild);
                if (emote != null) {
                    try {
                        await message.AddReactionAsync(emote);
                        await ConsoleHelper.Log("Added random Emote to a message!", LogSeverity.Info);
                    } catch {
                        // can't add reactions
                    }
                }
            }
        }
    }
}
