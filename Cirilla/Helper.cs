using Discord;
using Discord.Commands;
using System;
using System.Linq;

namespace Cirilla {
    public static class Helper {
        public static object Lock { get; set; } = new object();

        public static string GetName(IUser user) {
            if (user == null) {
                return "[Unknown]";
            }

            IGuildUser guildUser = user as IGuildUser;
            if (guildUser == null) {
                return string.IsNullOrWhiteSpace(user.Username) ? user.ToString() : user.Username;
            } else {
                return string.IsNullOrWhiteSpace(guildUser.Nickname) ? user.Username : guildUser.Nickname;
            }
        }

        public static Embed WrongCommand(IMessage message, CommandService service, ICommandContext context) {
            string command = message.Content
                .Replace(Information.Prefix.ToString(), "")
                .Replace(Information.SecondaryPrefix, "")
                .Replace(Information.Username, "");
            SearchResult searchResult = service.Search(context, command);
            if (searchResult.Commands != null && searchResult.Commands.Count > 0) {
                string nl = Environment.NewLine;
                bool multiple = searchResult.Commands.Count > 1;

                EmbedBuilder builder = new EmbedBuilder() {
                    Color = new Color(114, 137, 218),
                    Description = multiple ? "Did you mean some of these?" : "Did you mean this?"
                };

                foreach (CommandMatch match in searchResult.Commands) {
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

                return builder.Build();
            }

            return null;
        }
    }
}