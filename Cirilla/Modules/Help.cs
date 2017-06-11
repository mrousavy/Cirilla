using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Help : ModuleBase<CommandContext> {
        private CommandService _service;

        public Help(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        [Command("help")]
        public async Task HelpAsync() {
            char prefix = Information.Prefix;
            var builder = new EmbedBuilder() {
                Color = new Color(114, 137, 218),
                Description = "These are the commands you can use"
            };

            foreach (var module in _service.Modules) {
                string description = null;
                foreach (var cmd in module.Commands) {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"{prefix}{cmd.Aliases.First()} {string.Join(", ", cmd.Parameters.Select(p => p.Name))}" + Environment.NewLine;
                }

                if (!string.IsNullOrWhiteSpace(description)) {
                    builder.AddField(x => {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("help")]
        public async Task HelpAsync(string command) {
            var result = _service.Search(Context, command);

            if (!result.IsSuccess) {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            var builder = new EmbedBuilder() {
                Color = new Color(114, 137, 218),
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands) {
                var cmd = match.Command;

                builder.AddField(x => {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}" + Environment.NewLine +
                              $"Remarks: {cmd.Remarks}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}
