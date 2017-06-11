using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Votekick : ModuleBase {
        [Command("votekick"), Summary("Votekick a User")]
        public async Task VoteToKick([Summary("The user to kick")] IGuildUser user) {
            if (!Information.AllowVotekick) {
                await ReplyAsync("Votekick is disabled!");
                return;
            }

            try {
                if (user.Status != UserStatus.Online && user.Status != UserStatus.DoNotDisturb && user.Status != UserStatus.Invisible) {
                    await Context.Channel.SendMessageAsync($"You can't kick offline/afk users.. That's mean!");
                    return;
                }

                if (user.Id == Cirilla.Client.CurrentUser.Id) {
                    await ReplyAsync("Why do you guys wanna kick me? :cry:");
                    return;
                }

                IUserMessage message = await ReplyAsync(
                    $"{Helper.GetName(Context.User)} started a votekick on {user.Mention} - " +
                    "This vote expires in 30 seconds!");
                await message.AddReactionAsync(new Emoji(Information.VotekickYes));
                await message.AddReactionAsync(new Emoji(Information.VotekickNo));
                Cirilla.Client.ReactionAdded += ReactionAdded;

                Expire(user, message, Context.Guild);
            } catch { }
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> cachableMessage, ISocketMessageChannel channel, SocketReaction reaction) {
            try {
                if (channel == null)
                    return;

                IGuild guild = ((IGuildChannel)channel).Guild;
                IMessage message = await channel.GetMessageAsync(cachableMessage.Id);

                if (message.Author.Id == Cirilla.Client.CurrentUser.Id) {
                    ulong id = message.MentionedUserIds.FirstOrDefault();
                    IGuildUser user = await guild.GetUserAsync(id);
                    if (id != 0) {
                        Kick(user, (IUserMessage)message, guild);
                    }
                }
            } catch {
                // error
            }
        }

        private async void Expire(IGuildUser user, IUserMessage message, IGuild guild) {
            try {
                await Task.Delay(Information.VotekickExpire);
                Cirilla.Client.ReactionAdded -= ReactionAdded;

                Kick(user, message, guild);
            } catch {
                await message.Channel.SendMessageAsync($"Could not kick {Helper.GetName(user)}.. :confused:");
            }
        }


        private async void Kick(IGuildUser user, IUserMessage message, IGuild guild) {
            try {
                if (await guild.GetUserAsync(user.Id) == null) {
                    //already kicked by ReactionAdded event or server left
                    return;
                }

                List<IUser> yesUser = new List<IUser>(await message.GetReactionUsersAsync(Information.VotekickYes));
                List<IUser> noUser = new List<IUser>(await message.GetReactionUsersAsync(Information.VotekickNo));

                string nl = Environment.NewLine;

                // -1 for own reaction
                int yes = yesUser.Count - 1;
                int no = noUser.Count - 1;
                //only users in a voice channel can votekick
                int online = new List<IGuildUser>(((await guild.GetUsersAsync()).Where(u => (u.Status == UserStatus.Online && u.VoiceChannel != null)))).Count;

                //more than half of server users voted yes
                if (yes > online / 2 && no < yes) {
                    //check if admin voted no -> dismiss vote
                    foreach (IUser iuser in noUser) {
                        if (CheckIfAdmin(iuser as IGuildUser)) {
                            await message.Channel.SendMessageAsync($"An admin voted _no_, _{Helper.GetName(user)}_ cannot be kicked!");
                            return;
                        }
                    }

                    IInviteMetadata invite = await ((IGuildChannel)message.Channel).CreateInviteAsync(maxUses: 1);
                    try {
                        IDMChannel dm = await user.CreateDMChannelAsync();
                        await dm.SendMessageAsync($"You've been kicked from the _{guild.Name}_ guild by votekick!" + nl +
                            $"As I'm very generous today, here's an invite link to the _{guild.Name}_ guild:" + nl + invite.Url);
                    } catch {
                        //user is not allowing DMs?
                        await message.Channel.SendMessageAsync($"{Helper.GetName(user)} is not allowing private messages, " +
                            "someone gotta send him an invite link again.." + nl + invite.Url);
                    }
                    await user.KickAsync();
                    await message.Channel.SendMessageAsync($"You bullies kicked the poor {Helper.GetName(user)}..");
                    try {
                        Cirilla.Client.ReactionAdded -= ReactionAdded;
                    } catch {
                        //event removed
                    }
                } else {
                    await message.Channel.SendMessageAsync($"Time's up. Less than half of the online users ({yes}) " +
                        $"voted to kick {Helper.GetName(user)}. Votekick dismissed.");
                }
            } catch {
                await message.Channel.SendMessageAsync($"Could not kick {Helper.GetName(user)}.. :confused:");
            }
        }

        private bool CheckIfAdmin(IGuildUser user) {
            if (user == null)
                return false;
            return user.GuildPermissions.Administrator;
        }
    }
}
