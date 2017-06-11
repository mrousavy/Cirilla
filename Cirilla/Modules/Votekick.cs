using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Votekick : ModuleBase {
        [Command("votekick"), Summary("Votekick a User")]
        public async Task VoteToKick([Summary("The user to kick")] IUser user) {
            try {
                IUserMessage message = await ReplyAsync(
                    $"{Context.User.Username} started a votekick on {user.Username} - " +
                    "This vote expires in 30 seconds!");
                await message.AddReactionAsync(new Emoji(Information.VotekickYes));
                await message.AddReactionAsync(new Emoji(Information.VotekickNo));

                Expire((IGuildUser)user, message, Context.Guild);
            } catch {

            }
        }



        private async void Expire(IGuildUser user, IUserMessage message, IGuild guild) {
            try {
                await Task.Delay(Information.VotekickExpire);

                int yes = (await message.GetReactionUsersAsync(Information.VotekickYes)).Count;
                int no = (await message.GetReactionUsersAsync(Information.VotekickNo)).Count;
                int total = (await guild.GetUsersAsync()).Count;

                //more than half of server users voted yes
                if (yes > 1) {
                    IDMChannel dm = await user.CreateDMChannelAsync();
                    IDisposable disposable = dm.EnterTypingState();
                    await dm.SendMessageAsync($"You've been kicked from the _{guild.Name}_ guild by _{user.Username}_!");
                    await dm.SendMessageAsync($"As I'm very generous today, here's an invite link to the _{guild.Name}_");
                    IInviteMetadata invite = await ((IGuildChannel)message.Channel).CreateInviteAsync(maxUses: 1);
                    await dm.SendMessageAsync(invite.Url);
                    await user.KickAsync();
                    disposable.Dispose();
                }
            } catch {
                await message.Channel.SendMessageAsync($"Could not kick {user.Username}.. :confused:");
            }
        }
    }
}
