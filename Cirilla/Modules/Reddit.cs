using Discord;
using Discord.Commands;
using RedditNet;
using RedditNet.Things;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Reddit : ModuleBase {
        [Command("dankmeme"), Summary("Get random dank post from r/dankmemes")]
        public async Task Dankmeme() {
            try {
                RedditApi redditService = new RedditApi();
                Subreddit subreddit = await redditService.GetSubredditAsync("dankmemes");
                RedditNet.Things.Link link = await subreddit.GetRandomLinkAsync();
                await ReplyAsync(link.Title + " - " + link.Url);
                await ConsoleHelper.Log($"{Helper.GetName(Context.User)} requested a dank meme.", LogSeverity.Info);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Error getting random meme from /r/dankmemes! ({ex.Message})", LogSeverity.Error);
                await ReplyAsync("Sorry, I couldn't find a dank enough post!");
            }
        }

        //[Command("reddit"), Summary("Get random post from r/all")]
        //public async Task All() {
        //    try {
        //        RedditApi redditService = new RedditApi();
        //        Subreddit subreddit = await redditService.GetSubredditAsync("all");
        //        RedditNet.Things.Link link = await subreddit.GetRandomLinkAsync();
        //        await ReplyAsync(link.Url);
        //    } catch {
        //        await ReplyAsync("Sorry, I couldn't find a random post!");
        //    }
        //}

        [Command("reddit"), Summary("Get random post from a custom subreddit")]
        public async Task RandomPost([Summary("The subreddit to look for a random post")] string rsubreddit) {
            try {
                string filtered = Regex.Replace(rsubreddit, "/?r/", "");

                RedditApi redditService = new RedditApi();
                Subreddit subreddit = await redditService.GetSubredditAsync(filtered);
                RedditNet.Things.Link link = await subreddit.GetRandomLinkAsync();
                await ReplyAsync(link.Title + " - " + link.Url);
                await ConsoleHelper.Log($"{Helper.GetName(Context.User)} requested a random link from /r/{filtered}.", LogSeverity.Info);
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Error getting random link from {rsubreddit}! ({ex.Message})", LogSeverity.Error);
                await ReplyAsync("Sorry, I couldn't find a random post in that subreddit!");
            }
        }
    }
}
