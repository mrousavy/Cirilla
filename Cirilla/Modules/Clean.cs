using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Clean : ModuleBase {
        [Command("clean"), Summary("Clean the last 25 messages in the chat!")]
        public async Task CleanChat() {
            try {
                IGuildUser user = Context.User as IGuildUser;
                if (user == null) {
                    return;
                }
                if (!user.GuildPermissions.Has(GuildPermission.ManageMessages)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                IDisposable disposable = Context.Channel.EnterTypingState();
                IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(25).Flatten();
                await Context.Channel.DeleteMessagesAsync(messages);
                disposable.Dispose();
            } catch {
                await ReplyAsync("Whoops, unfortunately I couldn't clean the last messages.. :confused:");
            }
        }

        [Command("clean"), Summary("Clean the last x messages in the chat!")]
        public async Task CleanChat([Summary("The numer of messages to remove")] int count) {
            try {
                IGuildUser user = Context.User as IGuildUser;
                if (user == null) {
                    return;
                }
                if (!user.GuildPermissions.Has(GuildPermission.ManageMessages)) {
                    await ReplyAsync("Sorry, but you're not allowed to use that super premium command!");
                    return;
                }

                IDisposable disposable = Context.Channel.EnterTypingState();
                //++ because own delete own message aswell
                count++;
                IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(count).Flatten();
                await Context.Channel.DeleteMessagesAsync(messages);
                disposable.Dispose();
            } catch {
                await ReplyAsync("Whoops, unfortunately I couldn't clean the last messages.. :confused:");
            }
        }
    }
}
