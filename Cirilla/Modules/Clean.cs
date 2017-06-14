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
                await ConsoleHelper.Log($"{Context.User} cleaned the Chat (25 Messages)", LogSeverity.Info);
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't clean the last messages.. :confused:");
                await ConsoleHelper.Log($"Error cleaning Chat ({ex.Message})", LogSeverity.Error);
            }
        }

        [Command("clean"), Summary("Clean the last [x] messages in the chat!")]
        public async Task CleanChat([Summary("The number of messages to remove")] int count) {
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
                await ConsoleHelper.Log($"{Context.User} cleaned the Chat ({count} Messages)", LogSeverity.Info);
            } catch (Exception ex) {
                await ReplyAsync("Whoops, unfortunately I couldn't clean the last messages.. :confused:");
                await ConsoleHelper.Log($"Error cleaning Chat ({ex.Message})", LogSeverity.Error);
            }
        }
    }
}
