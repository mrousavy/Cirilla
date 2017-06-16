using Cirilla.Services.Profiles;
using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Profile : ModuleBase {
        [Command("profile"), Summary("Show a user's Profile")]
        public async Task ShowProfile([Summary("The user you want to show the Profile of")] IGuildUser user) {
            string text = ProfileManager.ReadProfile(user.Id);

            if (text == null) {
                await ReplyAsync($"{Helper.GetName(user)} does not have a custom profile set up!" +
                                 Environment.NewLine +
                                 $"He can setup a new profile by using the `{Information.Prefix}setprofile My Profile Description` command");
                return;
            }


            EmbedBuilder builder = new EmbedBuilder {
                Color = new Color(50, 125, 125),
                Author = new EmbedAuthorBuilder {
                    Name = $"{Helper.GetName(user)}'s Profile 📜",
                    IconUrl = user.GetAvatarUrl()
                }
            };
            builder.AddField("\u200B", text);

            await ReplyAsync("", embed: builder.Build());
            await ConsoleHelper.Log($"{Helper.GetName(Context.User)} requested {Helper.GetName(user)}'s profile.",
                LogSeverity.Info);
        }

        [Command("profile"), Summary("Show your own Profile")]
        public async Task ShowProfile() {
            IGuildUser user = Context.User as IGuildUser;
            if (user == null)
                return;

            string text = ProfileManager.ReadProfile(user.Id);

            if (text == null) {
                await ReplyAsync("You don't have a custom profile set up!" + Environment.NewLine +
                                 $"Setup a new profile by using the `{Information.Prefix}setprofile My Profile Description` command!");
                return;
            }


            EmbedBuilder builder = new EmbedBuilder {
                Color = new Color(50, 125, 125),
                Author = new EmbedAuthorBuilder {
                    Name = $"{Helper.GetName(user)}'s Profile 📜",
                    IconUrl = user.GetAvatarUrl()
                }
            };
            builder.AddField("\u200B", text);

            await ReplyAsync("", embed: builder.Build());
            await ConsoleHelper.Log($"{Helper.GetName(Context.User)} requested his profile.", LogSeverity.Info);
        }


        [Command("setprofile"), Summary("Set your custom user profile, this will be displayed in a Markdown Embed!")]
        public async Task SetProfile() {
            ProfileListener listener = new ProfileListener(Context.Channel as ITextChannel, Context.User);
            Cirilla.Client.MessageReceived += listener.ProfileTextReceived;

            await ReplyAsync("What text do you want to display on your Profile?");
            await ConsoleHelper.Log($"{Helper.GetName(Context.User)} is creating a new Profile..", LogSeverity.Info);
        }
    }
}