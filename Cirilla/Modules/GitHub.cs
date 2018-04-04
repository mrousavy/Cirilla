using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Cirilla.Modules
{
    public class GitHub : ModuleBase
    {
        [Command("bugreport")]
        [Summary("Report a Bug in Cirilla on GitHub")]
        public async Task Bugreport()
        {
            var builder = new EmbedBuilder
            {
                Color = new Color(244, 196, 65),
                ThumbnailUrl = Information.GitHubLogo
            };
            builder.AddField("Bug Report on GitHub",
                $"You've found a bug? Report it [here]({Information.RepoUrl + "/issues/new"})!");

            await ReplyAsync("", embed: builder.Build());
        }

        [Command("repo")]
        [Summary("Get Repository URL")]
        public async Task Repo()
        {
            await ReplyAsync(Information.RepoUrl);
        }

        [Command("addModule")]
        [Summary("Help making Cirilla better by adding Modules!")]
        public async Task AddModules()
        {
            var builder = new EmbedBuilder
            {
                Color = new Color(66, 179, 244),
                ThumbnailUrl = Information.GitHubLogo,
                Title = "Help making Cirilla better!"
            };
            builder.AddField("1. Fork it", $"Fork the Project on GitHub [here]({Information.RepoUrl + "/fork"})!");
            builder.AddField("2. Clone it", "Clone the Project with any git client " +
                                            "(git bash: `git clone http://github.com/YOURNAME/Cirilla`)!");
            builder.AddField("3. DNX Restore", "Run `dotnet restore` in the Cirilla Project Folder");
            builder.AddField("4. Open the Project", "Open the Project with Visual Studio " +
                                                    "(you need the [.NET Core Tools](https://www.microsoft.com/net/core))");
            builder.AddField("5. Add a new Module", "Add a new `.cs` File in the `Modules` Folder " +
                                                    $"(you can see an Example on how the File has to look [here]" +
                                                    $"({Information.RepoUrl + "/blob/master/Cirilla/Modules/Connection.cs"}))");
            builder.AddField("6. Push", "`Add` all files, `commit` and `push` with Git, then create a Pull Request " +
                                        "on your Repository to merge it into `mrousavy/Cirilla`!");

            await ReplyAsync("", embed: builder.Build());
        }
    }
}