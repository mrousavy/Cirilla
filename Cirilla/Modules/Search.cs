using Cirilla.Services.Wikipedia;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urban.NET;

namespace Cirilla.Modules {
    public class Search : ModuleBase {
        [Command("google"), Summary("Google something!")]
        public async Task GoogleSearch([Summary("The Google search query")] [Remainder] string query) {
            try {
                await ReplyAsync(LmgtfyQuery(query));
            } catch {
                await ReplyAsync("Whoops, couldn't google that for you.. Now you have to do it yourself! :confused:");
            }
        }

        [Command("define"), Summary("Define something! (using Urban dictionary)")]
        public async Task Define([Summary("The word(s) to define")] [Remainder] string words) {
            try {
                UrbanService service = new UrbanService();
                Data data = await service.Data(words);

                if (data.List.Length > 0) {
                    List list = data.List.First();

                    EmbedBuilder builder = new EmbedBuilder {
                        Author = new EmbedAuthorBuilder {
                            Name = $"Definition for \"{words}\"",
                            Url = list.Permalink
                        },
                        Color = new Color(239, 255, 0),
                        Title = "\u200B",
                        Footer = new EmbedFooterBuilder {
                            Text = $"By {list.Author} | Result 1/{data.List.Length}"
                        }
                    };
                    builder.AddField("Definition", list.Definition);
                    builder.AddField("Example", list.Example);

                    await ReplyAsync("", embed: builder.Build());
                } else {
                    await ReplyAsync($"I couldn't define _\"{words}\"_ for you, sorry!");
                }
            } catch {
                await ReplyAsync("Whoops, couldn't define that for you.. Now you have to do it yourself! :confused:");
            }
        }

        [Command("wiki"), Summary("Search something on Wikipedia!")]
        public async Task WikipediaSearch([Summary("The Google search query")] [Remainder] string query) {
            try {
                if (string.IsNullOrWhiteSpace(query)) {
                    await ReplyAsync(
                        $"You can't search for nothing.. Usage example: `{Information.Prefix}wiki Computer`");
                    return;
                }

                WikipediaResponse response = await new WikipediaService().GetWikipediaResultsAsync(query);

                // Empty response.
                if (response?.Query == null || !response.Query.Pages.Any()) {
                    await ReplyAsync($"Failed to find anything for \"{query}\" on Wikipedia!");
                    return;
                }

                // Concat responses
                StringBuilder messageBuilder = new StringBuilder();
                response.Query.Pages.Values.ToList().ForEach(p => messageBuilder.AppendLine(p.Extract));
                string message = messageBuilder.ToString();

                // Double check
                if (message.Length == 0 || message == Environment.NewLine) {
                    await ReplyAsync($"Failed to find anything for \"{query}\" on Wikipedia!");
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
                                Name = $"Wikipedia results for \"{query}\" (part {i + 1})",
                                IconUrl =
                                    "https://www.wikipedia.org/portal/wikipedia.org/assets/img/Wikipedia-logo-v2.png"
                            },
                            Color = new Color(255, 255, 255),
                            Title = "\u200B",
                            Description = message.Substring(cursor,
                                (i == batchCount - 1) ? message.Length - cursor : DiscordConfig.MaxMessageSize)
                        };
                        await ReplyAsync("", embed: builder.Build());
                        cursor += DiscordConfig.MaxMessageSize;
                    }
                } else {
                    EmbedBuilder builder = new EmbedBuilder {
                        Author = new EmbedAuthorBuilder {
                            Name = $"Wikipedia results for \"{query}\"",
                            IconUrl = "https://www.wikipedia.org/portal/wikipedia.org/assets/img/Wikipedia-logo-v2.png"
                        },
                        Color = new Color(255, 255, 255),
                        Title = "\u200B",
                        Description = message
                    };

                    await ReplyAsync("", embed: builder.Build());
                }
                await ConsoleHelper.Log($"{Context.User} requested a Wikipedia article about \"{query}\"",
                    LogSeverity.Info);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Error on getting wikipedia article! ({ex.Message})", LogSeverity.Error);
                await ReplyAsync("Whoops, couldn't find that for you.. Now you have to do it yourself! :confused:");
            }
        }

        private static string LmgtfyQuery(string query) {
            query = System.Net.WebUtility.UrlEncode(query);
            return $"http://lmgtfy.com/?q={query}";
        }
    }
}
