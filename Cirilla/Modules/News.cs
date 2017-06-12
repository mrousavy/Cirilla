using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RedditNet;
using RedditNet.Things;

namespace Cirilla.Modules {
    public class News : ModuleBase {
        [Command("news"), Summary("Get top 5 Hot stories on r/news")]
        public async Task RedditNews() {
            try {
                const string loadingStr = "Loading top stories.. ({0}/5) :newspaper2:";
                IUserMessage loadingMsg = await ReplyAsync(string.Format(loadingStr, 0));

                List<RedditNet.Things.Link> links = await HotNews(5);
                for (int i = 0; i < links.Count; i++) {
                    EmbedBuilder builder = new EmbedBuilder {
                        Author = new EmbedAuthorBuilder {
                            Name = $"Reddit's Hot Story #{i + 1} 📰"
                        },
                        ThumbnailUrl = "https://raw.githubusercontent.com/mrousavy/Cirilla/master/Resources/Reddit_news.png",
                        Color = new Color(119, 200, 255)
                    };

                    builder.AddField(links[i].Title, links[i].Url ?? links[i].SelfText);
                    await ReplyAsync("", embed: builder.Build());
                    int i1 = i;
                    await loadingMsg.ModifyAsync(mp => {
                        mp.Content = string.Format(loadingStr, i1 + 1);
                    });
                }

                await loadingMsg.DeleteAsync();
            } catch {
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

                const string loadingStr = "Loading top stories.. ({0}/5) :newspaper2:";
                IUserMessage loadingMsg = await ReplyAsync(string.Format(loadingStr, 0));

                List<RedditNet.Things.Link> links = await HotNews((int)limit);
                for (int i = 0; i < links.Count; i++) {
                    EmbedBuilder builder = new EmbedBuilder {
                        Author = new EmbedAuthorBuilder {
                            Name = $"Reddit's Hot Story #{i + 1} 📰"
                        },
                        ThumbnailUrl = "https://raw.githubusercontent.com/mrousavy/Cirilla/master/Resources/Reddit_news.png",
                        Color = new Color(119, 200, 255)
                    };

                    builder.AddField(links[i].Title, links[i].Url ?? links[i].SelfText);
                    await ReplyAsync("", embed: builder.Build());
                    int i1 = i;
                    await loadingMsg.ModifyAsync(mp => {
                        mp.Content = string.Format(loadingStr, i1 + 1);
                    });
                }

                await loadingMsg.DeleteAsync();
            } catch {
                await ReplyAsync("Sorry, I couldn't find the newsfeed for today!");
            }
        }



        public static async void DailyNews(ITextChannel channel) {
            try {
                RedditNet.Things.Link link;
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
                Information.Config.LastPost = DateTime.Now.Day;
                Information.WriteOut();
            } catch {
                // no news for today :/
            }
        }

        public static void Init() { new Thread(TimerLoop).Start(); }


        public static async void TimerLoop() {
            while (true) {
                Thread.Sleep(Information.NewsInterval * 60 * 60 * 1000);
                await ConsoleHelper.Log("Fetching news..", LogSeverity.Info);
                foreach (IGuild guild in Cirilla.Client.Guilds) {
                    try {
                        ITextChannel channel = await guild.GetDefaultChannelAsync();
                        if (channel != null)
                            DailyNews(channel);
                    } catch {
                        // could not send news
                    }
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static async Task<List<RedditNet.Things.Link>> HotNews(int limit, string after = null) {
            RedditApi redditService = new RedditApi();
            Subreddit subreddit = await redditService.GetSubredditAsync("news");
            Listing listings = await subreddit.GetHotLinksAsync(new RedditNet.Requests.ListingRequest { Limit = limit, After = after });

            return listings.Select((t, i) => (RedditNet.Things.Link)listings.Children[i]).ToList();
        }





        //public static async Task<ItemNews[]> GetNewsContent(string NewsParameters) {

        //    List<ItemNews> Details = new List<ItemNews>();

        //    // httpWebRequest with API URL
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create
        //        ("http://news.google.com/news?q=" + NewsParameters + "&output=rss");

        //    //Method GET
        //    request.Method = "GET";

        //    //HttpWebResponse for result
        //    HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());


        //    //Mapping of status code
        //    if (response.StatusCode == HttpStatusCode.OK) {
        //        Stream receiveStream = response.GetResponseStream();
        //        StreamReader readStream = null;

        //        readStream = new StreamReader(receiveStream);

        //        //Get news data in json string

        //        string data = readStream.ReadToEnd();

        //        //Declare DataSet for putting data in it.
        //        DataSet ds = new DataSet();
        //        StringReader reader = new StringReader(data);
        //        ds.ReadXml(reader);
        //        DataTable dtGetNews = new DataTable();

        //        if (ds.Tables.Count > 3) {
        //            dtGetNews = ds.Tables["item"];

        //            foreach (DataRow dtRow in dtGetNews.Rows) {
        //                ItemNews DataObj = new ItemNews();
        //                DataObj.title = dtRow["title"].ToString();
        //                DataObj.link = dtRow["link"].ToString();
        //                DataObj.item_id = dtRow["item_id"].ToString();
        //                DataObj.PubDate = dtRow["pubDate"].ToString();
        //                DataObj.Description = dtRow["description"].ToString();
        //                Details.Add(DataObj);
        //            }
        //        }
        //    }

        //    //Return News array 
        //    return Details.ToArray();
        //}

        ////Define Class to return news data
        //public class ItemNews {
        //    public string title { get; set; }
        //    public string link { get; set; }
        //    public string item_id { get; set; }
        //    public string PubDate { get; set; }
        //    public string Description { get; set; }
        //}
    }
}
