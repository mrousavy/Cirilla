using Cirilla.Services;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Search : ModuleBase {
        [Command("google"), Summary("Google something!")]
        public async Task GoogleSearch([Summary("The Google search query")] params string[] query) {
            try {
                await ReplyAsync("" + Environment.NewLine + LmgtfyQuery(query));
            } catch {
                await ReplyAsync("Whoops, couldn't google that for you.. Now you have to do it yourself! :confused:");
            }
        }


        [Command("wiki"), Summary("Search something on Wikipedia!")]
        public async Task WikipediaSearch([Summary("The Google search query")] params string[] query) {
            try {
                string joined = string.Join(" ", query);
                joined = System.Net.WebUtility.UrlEncode(joined);

                WikipediaResponse response = await new WikipediaService().GetWikipediaResultsAsync(joined);

                // Empty response.
                if (response == null || response.Query == null || !response.Query.Pages.Any()) {
                    await ReplyAsync($"Failed to find anything for \"{joined}\" on Wikipedia!");
                    return;
                }

                // Concat responses
                StringBuilder messageBuilder = new StringBuilder();
                response.Query.Pages.Values.ToList().ForEach(p => messageBuilder.AppendLine(p.Extract));
                string message = messageBuilder.ToString();

                // Double check
                if (message.Length == 0 || message == Environment.NewLine) {
                    await ReplyAsync($"Failed to find anything for \"{joined}\" on Wikipedia!");
                    return;
                }

                // Discord has a limit on channel message size
                if (message.Length > DiscordConfig.MaxMessageSize) {
                    // IE: 5000 / 2000 = 2.5 ~= 3
                    decimal batchCount = Math.Ceiling(decimal.Divide(message.Length, DiscordConfig.MaxMessageSize));

                    // already sent
                    int cursor = 0;

                    for (int i = 0; i < batchCount; i++) {
                        EmbedBuilder builder = new EmbedBuilder {

                            Author = new EmbedAuthorBuilder {
                                Name = $"Wikipedia results for \"{joined}\" (pt {i + 1})",
                                IconUrl = "https://www.wikipedia.org/portal/wikipedia.org/assets/img/Wikipedia-logo-v2.png"
                            },
                            Color = new Color(255, 255, 255),
                            Title = "\u200B",
                            Description = message.Substring(cursor, (i == batchCount - 1) ? message.Length - cursor : DiscordConfig.MaxMessageSize)
                        };
                        await ReplyAsync("", embed: builder.Build());
                        cursor += DiscordConfig.MaxMessageSize;
                    }
                } else {
                    EmbedBuilder builder = new EmbedBuilder {
                        Author = new EmbedAuthorBuilder {
                            Name = $"Wikipedia results for \"{joined}\"",
                            IconUrl = "https://www.wikipedia.org/portal/wikipedia.org/assets/img/Wikipedia-logo-v2.png"
                        },
                        Color = new Color(255, 255, 255),
                        Title = "\u200B",
                        Description = message
                    };

                    await ReplyAsync("", embed: builder.Build());
                }
                await ConsoleHelper.Log($"{Context.User} requested a Wikipedia article about \"{joined}\"", LogSeverity.Info);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Error on getting wikipedia article! ({ex.Message})", LogSeverity.Error);
                await ReplyAsync("Whoops, couldn't find that for you.. Now you have to do it yourself! :confused:");
            }
        }

        private string LmgtfyQuery(params string[] tags) {
            string query = string.Join(" ", tags);
            query = System.Net.WebUtility.UrlEncode(query);
            return $"http://lmgtfy.com/?q={query}";
        }

        private string GoogleQuery(params string[] tags) {
            string query = string.Join(" ", tags);
            query = System.Net.WebUtility.UrlEncode(query);
            return $"https://www.google.at/#q={query}";
        }
    }
}
