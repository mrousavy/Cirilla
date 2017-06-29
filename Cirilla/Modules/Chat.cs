using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Chat : ModuleBase {
        [Command("hi"), Summary("Greets the User")]
        public async Task Hi() {
            await ReplyAsync("Hi! :wave: How are you doing?"); //ask user how he is doing (we don't care tho)
        }

        [Command("hello"), Summary("Greets the User")]
        public async Task Hello() {
            await ReplyAsync("Hi! :wave: How are you doing?"); //ask user how he is doing (we don't care tho)
        }

        [Command("poke"), Summary("Pokes a user ¯\\_(ツ)_/¯")]
        public async Task Poke([Summary("The User to poke")] IUser user) {
            try {
                if (user.IsBot) {
                    await ReplyAsync("You can't poke bots, silly! :rolling_eyes:");
                    return;
                }

                IDMChannel dm = await user.CreateDMChannelAsync();
                await dm.SendMessageAsync($"Hey! **{Context.User.Mention}** from **{Context.Guild.Name}** poked you!", true);
                await ReplyAsync($"Poked {Helper.GetName(user)}!");
            } catch {
                await ReplyAsync($"I couldn't poke {Helper.GetName(user)}, sorry!");
            }
        }

        [Command("poke"), Summary("Pokes a user ¯\\_(ツ)_/¯")]
        public async Task Poke([Summary("The User to poke")] IUser user, [Summary("The poke message")][Remainder] string message) {
            try {
                if (user.IsBot) {
                    await ReplyAsync("You can't poke bots, silly! :rolling_eyes:");
                    return;
                }

                IDMChannel dm = await user.CreateDMChannelAsync();
                await dm.SendMessageAsync($"Hey! **{Context.User.Mention}** from **{Context.Guild.Name}** poked you, saying: _\"{message}\"_", true);
                await ReplyAsync($"Poked {Helper.GetName(user)}!");
            } catch {
                await ReplyAsync($"I couldn't poke {Helper.GetName(user)}, sorry!");
            }
        }

        [Command("say"), Summary("Let the Bot say something")]
        public async Task Say([Summary("The text you want to let the bot say")] [Remainder] string text) {
            try {
                await Context.Message.DeleteAsync();
                await ReplyAsync(text);
            } catch {
                // can't delete?
            }
        }

        [Command("embed"), Summary("Let the Bot say something in an Embed")]
        public async Task Embed([Summary("The text you want to let the bot say")] [Remainder] string text) {
            if (text.Length > 2048) {
                await ReplyAsync("Maximum Length is 2048!");
            }

            await Context.Message.DeleteAsync();

            EmbedBuilder builder = new EmbedBuilder {
                Author = new EmbedAuthorBuilder {
                    Name = Helper.GetName(Context.User),
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Color = new Color(126, 160, 214),
                Description = text
            };

            await ReplyAsync("", embed: builder.Build());
        }
    }
}
