using System;
using System.Threading.Tasks;
using Cirilla.Services.Hardware;
using Discord;
using Discord.Commands;

namespace Cirilla.Modules
{
    public class Hardware : ModuleBase
    {
        [Command("hw")]
        [Summary("Show a user's Hardware")]
        public async Task ShowHardware([Summary("The user you want to show the Hardware of")]
            IGuildUser user)
        {
            try
            {
                var hwProfile = HardwareManager.ReadHardware(Context.Guild, user.Id);

                if (hwProfile == null)
                {
                    await ReplyAsync($"{Helper.GetName(user)} does not have his hardware set! " +
                                     $"Create a new hardware profile by using the `{Information.Prefix}sethw <HardwareProfileName>` command!");
                    return;
                }


                var builder = new EmbedBuilder
                {
                    Color = new Color(50, 125, 125),
                    Author = new EmbedAuthorBuilder
                    {
                        Name = $"{Helper.GetName(user)}'s Hardware 🖥️",
                        IconUrl = user.GetAvatarUrl()
                    }
                };
                builder.AddField(hwProfile.Item1, hwProfile.Item2);

                await ReplyAsync("", embed: builder.Build());
            } catch (Exception ex)
            {
                await ReplyAsync($"Sorry, I can't get hardware information for {Helper.GetName(user)}!");
                ConsoleHelper.Log($"Error getting hardware for {user} ({ex.Message})", LogSeverity.Error);
            }
        }


        [Command("hw")]
        [Summary("Show own Hardware")]
        public async Task ShowHardware()
        {
            try
            {
                var user = Context.User as IGuildUser;
                if (user == null)
                    return;

                var hwProfile = HardwareManager.ReadHardware(Context.Guild, user.Id);

                if (hwProfile == null)
                {
                    await ReplyAsync("You don't have any hardware info set! " +
                                     $"Use `{Information.Prefix}sethw <HardwareProfileName>` to create a new Hardware Profile!");
                    return;
                }


                var builder = new EmbedBuilder
                {
                    Color = new Color(50, 125, 125),
                    Author = new EmbedAuthorBuilder
                    {
                        Name = $"{Helper.GetName(user)}'s Hardware 🖥️",
                        IconUrl = user.GetAvatarUrl()
                    }
                };
                builder.AddField(hwProfile.Item1, hwProfile.Item2);

                await ReplyAsync("", embed: builder.Build());
            } catch (Exception ex)
            {
                await ReplyAsync("Sorry, I can't show your hardware!");
                ConsoleHelper.Log($"Error getting hardware for {Context.User} ({ex.Message})", LogSeverity.Error);
            }
        }

        [Command("sethw")]
        [Summary("Set your custom hardware, this will be displayed in a Markdown Embed!")]
        public async Task SetHardware([Summary("The hardware title")] [Remainder]
            string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                await ReplyAsync(
                    $"Please enter a valid hardware title! Usage: `{Information.Prefix}sethw <HardwareProfileName>`");
                return;
            }

            var listener = new HardwareListener(Context.Channel as ITextChannel, Context.User);
            Cirilla.Client.MessageReceived += listener.HardwareReceived;
            HardwareManager.UpdateHardware(Context.Guild, Context.User.Id, title, null);

            ConsoleHelper.Log($"{Context.User} is creating a new hardware profile (\"{title}\")",
                LogSeverity.Info);

            await ReplyAsync(
                $"What text do you want to display on the hardware profile _{title}_? (Hint: Use `Shift` + `Enter` for new lines)");
        }
    }
}