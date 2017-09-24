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
        public static async Task<Tuple<Embed, string>> GetDailyNews() {
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
                    return null;
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
            return new Tuple<Embed, string>(builder.Build(), link.FullName);
        }

        public static void Init() {
            new Thread(TimerLoop).Start();
            ConsoleHelper.Log("News Service started!", LogSeverity.Info);
        }


        public static async void TimerLoop() {
            while (true) {
                DateTime nextPost = Information.LastPost.AddHours(Information.NewsInterval);
                DateTime now = DateTime.Now;
                TimeSpan diff;

                if (nextPost > now) {
                    diff = nextPost - now;
                } else {
                    // diff = now - nextPost;
                    Information.Config.LastPost = DateTime.Now.AddHours(Information.NewsInterval);
                    Information.WriteOut();
                    await ConsoleHelper.Log($"Next news post was in the past, resetted it to {Information.LastPost}!", LogSeverity.Error);
                    continue;
                }

                double sleep = Math.Abs(diff.TotalMilliseconds);

                if (sleep < 0) {
                    Thread.Sleep(1000);
                    return;
                }

                await ConsoleHelper.Log($"Next News in {diff}.. I'm going to sleep!", LogSeverity.Info);
                Thread.Sleep((int)sleep);
                await ConsoleHelper.Log("Fetching news..", LogSeverity.Info);

                try {
                    Tuple<Embed, string> result = await GetDailyNews();
                    if (result.Equals(default(Tuple<Embed, string>)))
                        throw new NullReferenceException($"{nameof(result)} is null");

                    foreach (IGuild guild in Cirilla.Client.Guilds) {
                        try {
                            ITextChannel channel = await guild.GetDefaultChannelAsync();
                            if (channel != null) {
                                await channel.SendMessageAsync("", embed: result.Item1);
                                await ConsoleHelper.Log($"Posted daily news in #{channel.Name}!", LogSeverity.Info);
                            }
                        } catch (Exception ex) {
                            await ConsoleHelper.Log($"Could not send news in #{guild.Name} ({ex.Message})!", LogSeverity.Info);
                        }
                    }

                    //Save that the last sent article was now
                    Information.Config.LastArticle = result.Item2;
                    Information.Config.LastPost = DateTime.Now;
                    Information.WriteOut();
                } catch (Exception ex) {
                    await ConsoleHelper.Log($"Could not get daily news! ({ex.Message})", LogSeverity.Error);
                }
            }
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