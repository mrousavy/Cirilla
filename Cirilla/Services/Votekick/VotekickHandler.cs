﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Cirilla.Services.Votekick
{
    //Individual handler for each reactions
    public class VotekickHandler
    {
        private readonly CancellationTokenSource _cts;
        private readonly IGuild _guild;
        private readonly IUserMessage _message;
        private readonly IGuildUser _user;


        public VotekickHandler(IGuildUser user, IUserMessage message, IGuild guild)
        {
            _user = user;
            _message = message;
            _guild = guild;
            _cts = new CancellationTokenSource();
            Expire();
        }


        private async void Expire()
        {
            try
            {
                //Wait until time's up
                await Task.Delay(Information.VotekickExpire, _cts.Token);
                Cirilla.Client.ReactionAdded -= ReactionAdded;

                if (await HasMajority())
                {
                    ConsoleHelper.Log($"Kicking {Helper.GetName(_user)}..", LogSeverity.Info);
                    var sentInvite = await DmInvite();
                    try
                    {
                        await Kick();
                    } catch
                    {
                        //delete invite again if he couldn't be kicked
                        await sentInvite.DeleteAsync();
                        //rethrow for outer exception handler
                        throw;
                    }

                    ConsoleHelper.Log($"Kicked {Helper.GetName(_user)}!", LogSeverity.Info);
                } else
                {
                    await _message.Channel.SendMessageAsync(
                        $"Time's up! :alarm_clock: Not enough users voted to kick {Helper.GetName(_user)}. Votekick dismissed."
                    );
                }
            } catch (TaskCanceledException)
            {
                // Expire cancelled, already kicked by ReactionAdded Event
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Could not kick {Helper.GetName(_user)}! ({ex.Message})", LogSeverity.Error);
                await _message.Channel.SendMessageAsync($"Could not kick {Helper.GetName(_user)}.. :confused:");
            }
        }

        public async Task ReactionAdded(Cacheable<IUserMessage, ulong> cachableMessage, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            try
            {
                if (channel == null)
                    return;

                if (await channel.GetMessageAsync(cachableMessage.Id) is IUserMessage message)
                    if (message.Id == _message.Id)
                        if (await HasMajority())
                        {
                            var sentInvite = await DmInvite();
                            try
                            {
                                await Kick();
                            } catch
                            {
                                //delete invite again if he couldn't be kicked
                                await sentInvite.DeleteAsync();
                                //rethrow for outer exception handler
                                throw;
                            }
                        }
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Error processing reaction! ({ex.Message})", LogSeverity.Error);
            }
        }

        //Is User Admin?
        private static bool CheckIfAdmin(IGuildUser user) => user != null && user.GuildPermissions.Administrator;

        //Is the majority of the Guild for yes?
        public async Task<bool> HasMajority()
        {
            //Get total users and all voters
            var totalUsers = await _guild.GetUsersAsync();
            var yesUser = await _message.GetReactionUsersAsync(Information.VotekickYes);
            var noUser = await _message.GetReactionUsersAsync(Information.VotekickNo);
            int yes = yesUser.Count;
            int no = noUser.Count;
            int online = totalUsers.Count(u => u.VoiceChannel != null);

            //more than half of online users voted yes AND
            //only half of the yes-voters voted for no
            if (yes > online / 2 && yes > no * 2)
            {
                //check if admin voted no -> dismiss vote
                if (noUser.Any(iuser => CheckIfAdmin(iuser as IGuildUser)))
                {
                    await _message.Channel.SendMessageAsync(
                        $"An admin voted _no_, _{Helper.GetName(_user)}_ cannot be kicked!");
                    return false;
                }

                return true;
            }

            return false;
        }

        //Send user an Invite via DM
        private async Task<IUserMessage> DmInvite()
        {
            string nl = Environment.NewLine;
            var invite = await ((IGuildChannel) _message.Channel).CreateInviteAsync(maxUses: 1);
            try
            {
                //DM the invite
                var dm = await _user.GetOrCreateDMChannelAsync();
                return await dm.SendMessageAsync($"You've been kicked from the _{_guild.Name}_ guild by votekick!" +
                                                 nl +
                                                 $"As I'm very generous today, here's an invite link to the _{_guild.Name}_ guild:" +
                                                 nl + invite.Url);
            } catch
            {
                //user is not allowing DMs?
                return await _message.Channel.SendMessageAsync(
                    $"{Helper.GetName(_user)} is not allowing private messages, " +
                    "someone @here gotta send him an invite link again.." + nl + invite.Url);
            }
        }

        //Actually kick the user
        private async Task Kick()
        {
            try
            {
                _cts.Cancel();

                if (await _guild.GetUserAsync(_user.Id) == null) return;
                await _user.KickAsync();
                await _message.Channel.SendMessageAsync($"You bullies kicked the poor {Helper.GetName(_user)}..");
                try
                {
                    Cirilla.Client.ReactionAdded -= ReactionAdded;
                } catch
                {
                    //event already removed
                }
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Could not kick {Helper.GetName(_user)}! ({ex.Message})", LogSeverity.Error);
                await _message.Channel.SendMessageAsync($"Could not kick {Helper.GetName(_user)}.. :confused:");
            }
        }
    }
}