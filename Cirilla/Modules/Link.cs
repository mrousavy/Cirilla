using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Link : ModuleBase {
        [Command("links"), Summary("Displays all saved links")]
        public async Task Command() {
            try {
                string file = Helper.GetPath(Context.Guild, "links.txt");
                if (File.Exists(file)) {
                    string links = File.ReadAllText(file);
                    EmbedBuilder builder = new EmbedBuilder {
                        Color = new Color(66, 134, 244)
                    };
                    builder.AddField(":link: Links:", links);
                    await ReplyAsync("", embed: builder.Build());
                } else {
                    await ReplyAsync(
                        "No Links are saved yet! Start adding links with `$addlink [Name](http://url.com)`!");
                }
                await ConsoleHelper.Log($"{Context.User} requested links.", LogSeverity.Info);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not get links! ({ex.Message})", LogSeverity.Error);
                await ReplyAsync("Whoops, can't show you my links right now.. :confused:");
            }
        }

        [Command("addlink"), Summary("Saves a new link")]
        public async Task AddLink([Summary("The link to store")] string link) {
            try {
                if (!((IGuildUser) Context.User).GuildPermissions.Administrator) {
                    await ReplyAsync($"You're not allowed to add links! Ask {Information.Owner}!");
                    return;
                }

                string file = Helper.GetPath(Context.Guild, "links.txt");
                File.AppendAllLines(file, new[] {link});
                await ConsoleHelper.Log($"{Context.User} added a new link (\"{link}\")", LogSeverity.Info);
                await ReplyAsync("Link added! :link:");
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not save link! {ex.Message}", LogSeverity.Error);
                await ReplyAsync("Whoops, couldn't save that link.. :confused:");
            }
        }
    }
}