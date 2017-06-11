using Discord;
using Discord.Commands;
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
        }


        [Command("setprofile"), Summary("Set your custom user profile, this will be displayed in a Markdown Embed!")]
        public async Task SetProfile([Summary("The text you want to display on your Profile")] params string[] arguments) {
            string text = string.Join(" ", arguments);
            UpdateProfile(Context.User.Id, text);
            await ReplyAsync("Profile updated successfully!");
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
            Profiles.Profiles.Add(new UserProfile(userId, text));

            if (!File.Exists(profiles)) {
                File.WriteAllText(profiles, JsonConvert.SerializeObject(Profiles));
            } else {
                for (int i = 0; i < Profiles.Profiles.Count; i++) {
                    if (Profiles.Profiles[i].UserId == userId) {
                        Profiles.Profiles[i].ProfileText = text;
                        File.WriteAllText(profiles, JsonConvert.SerializeObject(Profiles));
                        return;
                    }
                }
            }
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
