﻿using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Help : ModuleBase<CommandContext> {
        private readonly CommandService _service;

        public Help(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        [Command("help"), Summary("Show all available Commands")]
        public async Task HelpAsync() {
            char prefix = Information.Prefix;
            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(114, 137, 218),
                Description = "These are the commands you can use:"
            };

            foreach (ModuleInfo module in _service.Modules) {
                string description = null;
                foreach (var cmd in module.Commands) {
                    PreconditionResult result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess) {
                        description += $"{prefix}{cmd.Aliases.First()} {string.Join(", ", cmd.Parameters.Select(p => p.Name))}" + Environment.NewLine;
                    }
                }

                if (!string.IsNullOrWhiteSpace(description)) {
                    builder.AddField(x => {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            if (Information.PmHelp) {
                try {
                    IDMChannel dm = await Context.User.CreateDMChannelAsync();
                    await dm.SendMessageAsync("", false, builder.Build());
                } catch {
                    //could not send private
                    await ReplyAsync("", false, builder.Build());
                }
            } else {
                await ReplyAsync("", false, builder.Build());
            }
        }

        [Command("help"), Summary("Show information and usage about a command")]
        public async Task HelpAsync(string command) {
            SearchResult result = _service.Search(Context, command);
            string nl = Environment.NewLine;

            if (!result.IsSuccess) {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(114, 137, 218),
                Description = $"Here are some commands like **{command}**:"
            };

            foreach (CommandMatch match in result.Commands) {
                CommandInfo cmd = match.Command;

                builder.AddField(x => {
                    x.Name = $"{Information.Prefix}{cmd.Aliases.First()} {string.Join(" ", cmd.Parameters)}";
                    if (cmd.Parameters.Count < 1) {
                        x.Value = $"Summary: {cmd.Summary}";
                    } else {
                        x.Value = $"Summary: {cmd.Summary}" + nl +
                                  $"Parameters: {nl}\t{string.Join($"\t{nl}", cmd.Parameters.Select(p => $"_{p.Name}_: {p.Summary}"))}";
                    }
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}
