using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Greetings : ModuleBase {
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
                IDMChannel dm = await user.CreateDMChannelAsync();
                await dm.SendMessageAsync($"Hey! **{Context.User}** from **{Context.Guild.Name}** poked you!", true);
                await ReplyAsync($"Poked {Helper.GetName(user)}!");
            } catch {
                await ReplyAsync($"I couldn't poke {Helper.GetName(user)}, sorry!");
            }
        }
    }
}