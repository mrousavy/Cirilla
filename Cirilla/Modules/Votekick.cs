using Cirilla.Services.Votekick;
using Discord;
using Discord.Commands;
using System;
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
                if (!(await Context.Guild.GetCurrentUserAsync()).GuildPermissions.KickMembers) {
                    await ReplyAsync(
                        $"I can't kick people here, talk to {(await Context.Guild.GetOwnerAsync()).Mention}!");
                    return;
                }

                if (user.Status != UserStatus.Online && user.Status != UserStatus.DoNotDisturb &&
                    user.Status != UserStatus.Invisible) {
                    await Context.Channel.SendMessageAsync("You can't kick offline/afk users.. That's mean!");
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

                VotekickHandler handler = new VotekickHandler(user, message, Context.Guild);
                Cirilla.Client.ReactionAdded += handler.ReactionAdded;
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not start votekick {Helper.GetName(user)}! ({ex.Message})",
                    LogSeverity.Error);
            }
        }
    }
}