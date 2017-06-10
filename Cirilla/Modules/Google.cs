using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Google : ModuleBase {
        [Command("google"), Summary("Google something!")]
        public async Task GoogleLink([Summary("The Google search query")] params string[] query) {
            try {
                await ReplyAsync("" + Environment.NewLine + LmgtfyQuery(query));
            } catch {
                await ReplyAsync("Whoops, couldn't google that for you.. Now you have to do it yourself! :confused:");
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
