using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Profile : ModuleBase {
        [Command("profile"), Summary("Show a user's Profile")]
        public async Task ShowProfile([Summary("The user you want to show the Profile of")]IGuildUser user) {
            string text = ReadProfile(user.Id);

            if (text == null) {
                await ReplyAsync($"{Helper.GetName(user)} does not have a custom profile set up!" + Environment.NewLine +
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
            await ConsoleHelper.Log($"{Helper.GetName(Context.User)} requested {Helper.GetName(user)}'s profile.", LogSeverity.Info);
        }

        [Command("profile"), Summary("Show your own Profile")]
        public async Task ShowProfile() {
            IGuildUser user = Context.User as IGuildUser;
            if (user == null)
                return;

            string text = ReadProfile(user.Id);

            if (text == null) {
                await ReplyAsync($"You don't have a custom profile set up!" + Environment.NewLine +
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

            await ReplyAsync($"What text do you want to display on your Profile?");
            await ConsoleHelper.Log($"{Helper.GetName(Context.User)} is creating a new Profile..", LogSeverity.Info);
        }



        public class ProfileListener {
            private ITextChannel _channel;
            private IUser _user;
            private bool _done;

            public ProfileListener(ITextChannel channel, IUser user) {
                _channel = channel;
                _user = user;
                Hourglass();
            }

            public async Task ProfileTextReceived(SocketMessage arg) {
                try {
                    SocketUserMessage message = arg as SocketUserMessage;
                    if (message == null)
                        return;

                    IUser user = message.Author;
                    ITextChannel channel = message.Channel as ITextChannel;
                    if (user.Id == _user.Id && channel.Id == _channel.Id) {
                        UpdateProfile(user.Id, message.Content);

                        Cirilla.Client.MessageReceived -= ProfileTextReceived;
                        _done = true;
                        await arg.Channel.SendMessageAsync("Profile set successfully!");
                        await ConsoleHelper.Log($"{Helper.GetName(_user)} created a new profile!", LogSeverity.Info);
                    }
                } catch (Exception ex) {
                    await ConsoleHelper.Log($"Could not create profile! ({ex.Message})", LogSeverity.Error);
                    await arg.Channel.SendMessageAsync("Sorry, I couldn't set your Profile Information :confused:");
                }
            }

            public async void Hourglass() {
                //time in Minutes until the bot stops listening
                int time = 5;
                await Task.Delay(1000 * 60 * time);
                if (!_done) {
                    Cirilla.Client.MessageReceived -= ProfileTextReceived;
                    await _channel.SendMessageAsync("Sorry, I'm out of time! :hourglass:");
                }
            }
        }



        public static string ReadProfile(ulong userId) {
            string profiles = Path.Combine(Information.Directory, "profiles.json");
            if (File.Exists(profiles)) {
                Profiles = JsonConvert.DeserializeObject<UserProfiles>(File.ReadAllText(profiles));

                UserProfile profile = Profiles.Profiles.FirstOrDefault(p => p.UserId == userId);
                return profile?.ProfileText;
            } else {
                return null;
            }
        }

        public static void UpdateProfile(ulong userId, string text) {
            string profiles = Path.Combine(Information.Directory, "profiles.json");

            if (Profiles == null)
                Profiles = new UserProfiles();

            bool contains = false;

            for (int i = 0; i < Profiles.Profiles.Count; i++) {
                if (Profiles.Profiles[i].UserId == userId) {
                    Profiles.Profiles[i].ProfileText = text;
                    contains = true;
                    break;
                }
            }

            if (!contains) {
                Profiles.Profiles.Add(new UserProfile(userId, text));
            }

            File.WriteAllText(profiles, JsonConvert.SerializeObject(Profiles));
        }


        public static UserProfiles Profiles;

        public class UserProfiles {
            public UserProfiles() {
                Profiles = new List<UserProfile>();
            }

            public List<UserProfile> Profiles { get; set; }
        }

        public class UserProfile {
            public UserProfile(ulong id, string text) {
                UserId = id;
                ProfileText = text;
            }

            public ulong UserId { get; set; }
            public string ProfileText { get; set; }
        }
    }
}
