using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;

namespace Cirilla {
    public static class Helper {
        public static object Lock { get; } = new object();

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
                .Replace(Cirilla.Client.CurrentUser.Mention, "");
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

        /// <summary>
        /// Get the path to the file specified so it's guild specific
        /// </summary>
        public static string GetPath(IGuild guild, params string[] paths) {
            string[] fullpaths = new string[paths.Length + 2];
            fullpaths[0] = Information.Directory;
            fullpaths[1] = guild.Id.ToString();
            string directory = Path.Combine(fullpaths[0], fullpaths[1]);

            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "guildname.txt"),
                    $"\"{guild.Name}\" by <@{guild.OwnerId}> (created at {guild.CreatedAt}");
            }

            paths.CopyTo(fullpaths, 2);
            string path = Path.Combine(fullpaths);


            if (!File.Exists(path)) {
                File.Create(path).Dispose();
            }

            return path;
        }

        /// <summary>
        /// Get the path to the file specified so it's guild specific
        /// </summary>
        public static string GetPath(ulong guildId, params string[] paths) {
            string[] fullpaths = new string[paths.Length + 2];
            fullpaths[0] = Information.Directory;
            fullpaths[1] = guildId.ToString();
            string directory = Path.Combine(fullpaths[0], fullpaths[1]);

            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            paths.CopyTo(fullpaths, 2);
            string path = Path.Combine(fullpaths);


            if (!File.Exists(path)) {
                File.Create(path).Dispose();
            }

            return path;
        }


        public static bool IsOwner(IUser user) => $"<@{user.Id}>" == Information.Owner;
    }
}