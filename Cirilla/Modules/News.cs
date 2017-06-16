using Cirilla.Services.News;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class News : ModuleBase {
        [Command("news"), Summary("Get top 3 Hot stories on r/news")]
        public async Task RedditNews() {
            try {
                const string loadingStr = "Loading top stories.. ({0}/3) :newspaper2:";
                IUserMessage loadingMsg = await ReplyAsync(string.Format(loadingStr, 0));

                List<RedditNet.Things.Link> links = await NewsService.HotNews(3);
                for (int i = 0; i < links.Count; i++) {
                    EmbedBuilder builder = new EmbedBuilder {
                        Author = new EmbedAuthorBuilder {
                            Name = $"Reddit's Hot Story #{i + 1} 📰"
                        },
                        ThumbnailUrl =
                            "https://raw.githubusercontent.com/mrousavy/Cirilla/master/Resources/Reddit_news.png",
                        Color = new Color(119, 200, 255)
                    };

                    builder.AddField(links[i].Title, links[i].Url ?? links[i].SelfText);
                    await ReplyAsync("", embed: builder.Build());
                    int i1 = i;
                    await loadingMsg.ModifyAsync(mp => { mp.Content = string.Format(loadingStr, i1 + 1); });
                }

                await loadingMsg.DeleteAsync();
                await ConsoleHelper.Log($"{Helper.GetName(Context.User)} requested top 5 hot stories on r/news",
                    LogSeverity.Info);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not get top 5 hot stories on r/news! ({ex.Message})",
                    LogSeverity.Error);
                await ReplyAsync("Sorry, I couldn't find the newsfeed for today!");
            }
        }

        [Command("news"), Summary("Get Hot stories on r/news")]
        public async Task RedditNews([Summary("Limit for news articles")] uint limit) {
            try {
                if (limit > 15) {
                    await ReplyAsync("Sorry, you can't view more than 15 stories at once!");
                    return;
                }

                const string loadingStr = "Loading top stories.. ({0}/{1}) :newspaper2:";
                IUserMessage loadingMsg = await ReplyAsync(string.Format(loadingStr, 0, limit));

                List<RedditNet.Things.Link> links = await NewsService.HotNews((int) limit);
                for (int i = 0; i < links.Count; i++) {
                    EmbedBuilder builder = new EmbedBuilder {
                        Author = new EmbedAuthorBuilder {
                            Name = $"Reddit's Hot Story #{i + 1} 📰"
                        },
                        ThumbnailUrl =
                            "https://raw.githubusercontent.com/mrousavy/Cirilla/master/Resources/Reddit_news.png",
                        Color = new Color(119, 200, 255)
                    };

                    builder.AddField(links[i].Title, links[i].Url ?? links[i].SelfText);
                    await ReplyAsync("", embed: builder.Build());
                    int i1 = i;
                    await loadingMsg.ModifyAsync(mp => { mp.Content = string.Format(loadingStr, i1 + 1, limit); });
                }

                await loadingMsg.DeleteAsync();
                await ConsoleHelper.Log($"{Helper.GetName(Context.User)} requested top 5 hot stories on r/news",
                    LogSeverity.Info);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not get top 5 hot stories on r/news! ({ex.Message})",
                    LogSeverity.Error);
                await ReplyAsync("Sorry, I couldn't find the newsfeed for today!");
            }
        }
    }
}