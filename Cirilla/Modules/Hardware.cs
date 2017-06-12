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
    public class Hardware : ModuleBase {
        [Command("hw"), Summary("Show a user's Hardware")]
        public async Task ShowHardware([Summary("The user you want to show the Hardware of")]IGuildUser user) {
            try {
                Tuple<string, string> hwProfile = ReadHardware(user.Id);

                if (hwProfile == null) {
                    await ReplyAsync($"{Helper.GetName(user)} does not have his hardware set! " +
                        $"Create a new hardware profile by using the `{Information.Prefix}sethw <HardwareProfileName>` command!");
                    return;
                }


                EmbedBuilder builder = new EmbedBuilder {
                    Color = new Color(50, 125, 125),
                    Author = new EmbedAuthorBuilder {
                        Name = $"{Helper.GetName(user)}'s Hardware 🖥️",
                        IconUrl = user.GetAvatarUrl()
                    }
                };
                builder.AddField(hwProfile.Item1, hwProfile.Item2);

                await ReplyAsync("", embed: builder.Build());
            } catch (Exception ex) {
                await ReplyAsync($"Sorry, I can't get hardware information for {Helper.GetName(user)}!");
                await ConsoleHelper.Log($"Error getting hardware for {user} ({ex.Message})", LogSeverity.Error);
            }
        }


        [Command("hw"), Summary("Show own Hardware")]
        public async Task ShowHardware() {
            try {
                IGuildUser user = Context.User as IGuildUser;
                if (user == null)
                    return;

                Tuple<string, string> hwProfile = ReadHardware(user.Id);

                if (hwProfile == null) {
                    await ReplyAsync($"You don't have any hardware info set! " +
                        $"Use `{Information.Prefix}sethw <HardwareProfileName>` to create a new Hardware Profile!");
                    return;
                }


                EmbedBuilder builder = new EmbedBuilder {
                    Color = new Color(50, 125, 125),
                    Author = new EmbedAuthorBuilder {
                        Name = $"{Helper.GetName(user)}'s Hardware 🖥️",
                        IconUrl = user.GetAvatarUrl()
                    }
                };
                builder.AddField(hwProfile.Item1, hwProfile.Item2);

                await ReplyAsync("", embed: builder.Build());
            } catch (Exception ex) {
                await ReplyAsync($"Sorry, I can't show your hardware!");
                await ConsoleHelper.Log($"Error getting hardware for {Context.User} ({ex.Message})", LogSeverity.Error);
            }
        }

        [Command("sethw"), Summary("Set your custom hardware, this will be displayed in a Markdown Embed!")]
        public async Task SetHardware([Summary("The hardware title")] params string[] title) {
            if (title.Length < 1) {
                await ReplyAsync($"Please enter a valid hardware title! Usage: `{Information.Prefix}sethw <HardwareProfileName>`");
                return;
            }

            HardwareListener listener = new HardwareListener(Context.Channel as ITextChannel, Context.User);
            Cirilla.Client.MessageReceived += listener.HardwareReceived;
            string joined = string.Join(" ", title);
            UpdateHardware(Context.User.Id, joined, null);

            await ConsoleHelper.Log($"{Context.User} is creating a new hardware profile (\"{joined}\")", LogSeverity.Info);

            await ReplyAsync($"What text do you want to display on the hardware profile _{joined}_? (Hint: Use `Shift` + `Enter` for new lines)");
        }


        public class HardwareListener {
            private ITextChannel _channel;
            private IUser _user;
            private bool _done;

            public HardwareListener(ITextChannel channel, IUser user) {
                _channel = channel;
                _user = user;
                Hourglass();
            }

            public async Task HardwareReceived(SocketMessage arg) {
                try {
                    SocketUserMessage message = arg as SocketUserMessage;
                    if (message == null)
                        return;

                    IUser user = message.Author;
                    ITextChannel channel = message.Channel as ITextChannel;
                    if (user.Id == _user.Id && channel.Id == _channel.Id) {
                        UpdateHardware(user.Id, null, message.Content);

                        Cirilla.Client.MessageReceived -= HardwareReceived;
                        _done = true;
                        await ConsoleHelper.Log($"Created hardware profile for {_user}!", LogSeverity.Info);
                        await arg.Channel.SendMessageAsync("Hardware successfully set!");
                    }
                } catch (Exception ex) {
                    await ConsoleHelper.Log($"Could not create hardware profile for {_user}! ({ex.Message})", LogSeverity.Error);
                    await arg.Channel.SendMessageAsync("Sorry, I couldn't set your Hardware :confused:");
                }
            }

            public async void Hourglass() {
                //time in Minutes until the bot stops listening
                int time = 5;
                await Task.Delay(1000 * 60 * time);
                if (!_done) {
                    Cirilla.Client.MessageReceived -= HardwareReceived;
                    await _channel.SendMessageAsync($"Sorry {Helper.GetName(_user)}, I'm out of time! :hourglass:");
                }
            }
        }

        public static Tuple<string, string> ReadHardware(ulong userId) {
            string hwfile = Path.Combine(Information.Directory, "hardware.json");
            if (File.Exists(hwfile)) {
                UserHardwares = JsonConvert.DeserializeObject<UserHardwareFile>(File.ReadAllText(hwfile));

                UserHardware hw = UserHardwares.Hardwares.FirstOrDefault(h => h.UserId == userId);
                if (hw == null) {
                    return null;
                }
                return new Tuple<string, string>(hw.Title, hw.Hardware);
            } else {
                return null;
            }
        }

        public static void UpdateHardware(ulong userId, string title, string hardware) {
            string hwfile = Path.Combine(Information.Directory, "hardware.json");

            if (UserHardwares == null)
                UserHardwares = new UserHardwareFile();

            bool contains = false;

            for (int i = 0; i < UserHardwares.Hardwares.Count; i++) {
                if (UserHardwares.Hardwares[i].UserId == userId) {
                    if (!string.IsNullOrWhiteSpace(title))
                        UserHardwares.Hardwares[i].Title = title;
                    if (!string.IsNullOrWhiteSpace(hardware))
                        UserHardwares.Hardwares[i].Hardware = hardware;
                    contains = true;
                    break;
                }
            }

            if (!contains) {
                UserHardwares.Hardwares.Add(new UserHardware(userId, title, hardware));
            }

            File.WriteAllText(hwfile, JsonConvert.SerializeObject(UserHardwares));
        }


        public static UserHardwareFile UserHardwares;

        public class UserHardwareFile {
            public UserHardwareFile() {
                Hardwares = new List<UserHardware>();
            }

            public List<UserHardware> Hardwares { get; set; }
        }

        public class UserHardware {
            public UserHardware(ulong id, string title, string hardware) {
                UserId = id;
                Title = title;
                Hardware = hardware;
            }

            public ulong UserId { get; set; }
            public string Hardware { get; set; }
            public string Title { get; set; }
        }
    }
}
