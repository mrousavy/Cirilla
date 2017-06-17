using Discord;
using RedditNet;
using RedditNet.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cirilla.Services.News {
    public class NewsService {
        public static async void DailyNews(ITextChannel channel) {
            try {
                Link link;
                const int maxRetries = 10;
                int retries = 0;
                string lastName = null;
                do {
                    link = (await HotNews(1, lastName))[0];
                    lastName = link.FullName;

                    retries++;
                    if (retries >= maxRetries) {
                        // no news for today :/
                        return;
                    }

                    await ConsoleHelper.Log($"Article downloaded: {link.FullName}", LogSeverity.Info);
                } while (link.FullName == Information.LastArticle);

                EmbedBuilder builder = new EmbedBuilder {
                    Author = new EmbedAuthorBuilder {
                        Name = "Reddit's Hot Story for today! 📰"
                    },
                    ThumbnailUrl =
                        "https://raw.githubusercontent.com/mrousavy/Cirilla/master/Resources/Reddit_news.png",
                    Color = new Color(119, 200, 255)
                };

                builder.AddField(link.Title, link.Url ?? link.SelfText);
                await channel.SendMessageAsync("", embed: builder.Build());

                Information.Config.LastArticle = link.FullName;
                Information.Config.LastPost = DateTime.Now;
                Information.WriteOut();

                await ConsoleHelper.Log($"Posted daily news in #{channel.Name}!", LogSeverity.Info);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not get daily news! ({ex.Message})", LogSeverity.Error);
                // no news for today :/
            }
        }

        public static void Init() { new Thread(TimerLoop).Start(); }


        public static async void TimerLoop() {
            while (true) {
                DateTime nextPost = Information.LastPost.AddHours(Information.NewsInterval);
                DateTime now = DateTime.Now;
                TimeSpan diff;

                if (nextPost > now) {
                    diff = nextPost - now;
                } else {
                    diff = now - nextPost;
                }

                double sleep = Math.Abs(diff.TotalMilliseconds);

                if (sleep < 0) {
                    Thread.Sleep(1000);
                    return;
                }

                await ConsoleHelper.Log($"Next News in {diff.ToString()}.. I'm going to sleep!", LogSeverity.Info);
                Thread.Sleep((int)sleep);
                await ConsoleHelper.Log("Fetching news..", LogSeverity.Info);
                foreach (IGuild guild in Cirilla.Client.Guilds) {
                    try {
                        ITextChannel channel = await guild.GetDefaultChannelAsync();
                        if (channel != null) {
                            DailyNews(channel);
                        }
                    } catch {
                        // could not send news
                    }
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public static async Task<List<Link>> HotNews(int limit, string after = null) {
            RedditApi redditService = new RedditApi();
            Subreddit subreddit = await redditService.GetSubredditAsync("news");
            Listing listings =
                await subreddit.GetHotLinksAsync(new RedditNet.Requests.ListingRequest { Limit = limit, After = after });

            return listings.Select((t, i) => (Link)listings.Children[i]).ToList();
        }
    }
}