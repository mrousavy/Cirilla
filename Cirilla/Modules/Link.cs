﻿using Discord;
using Discord.Commands;
using System.IO;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Link : ModuleBase {
        [Command("links"), Summary("Displays all saved links")]
        public async Task Command() {
            try {
                EmbedBuilder builder = new EmbedBuilder {
                    Color = new Color(66, 134, 244)
                };

                string file = Path.Combine(Information.Directory, "links.txt");
                if (File.Exists(file)) {
                    string links = File.ReadAllText(file);
                    builder.AddField("Links:", links);
                    await ReplyAsync("", embed: builder.Build());
                } else {
                    await ReplyAsync($"No Links are saved yet! Start adding links with `$addlink [Name](http://url.com)`!");
                }
            } catch {
                await ReplyAsync("Whoops, can't show you my links right now.. :confused:");
            }
        }

        [Command("addlink"), Summary("Saves a new link")]
        public async Task AddLink([Summary("The link to store")] string link) {
            try {

                await ReplyAsync($"Link saved!");
            } catch {
                await ReplyAsync("Whoops, couldn't save that link.. :confused:");
            }
        }
    }
}