using System;
using System.Linq;
using System.Threading.Tasks;
using Cirilla.Services.GuildConfig;
using Discord;
using Discord.Commands;

namespace Cirilla.Modules
{
    public class Help : ModuleBase<CommandContext>
    {
        private readonly CommandService _service;

        public Help(CommandService service)
        {
            _service = service;
        }

        [Command("help")]
        [Summary("Show all available Commands")]
        public async Task HelpAsync()
        {
            char prefix = Information.Prefix;
            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = "These are the commands **you can use** here:",
                Footer = new EmbedFooterBuilder
                {
                    Text = "Use \"$help command\" to see more info about a specific Command!"
                },
                Title = "Help (Click here for a list of **all** available commands)",
                Url = "http://github.com/mrousavy/Cirilla#commands"
            };

            foreach (var module in _service.Modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    if (!CanUse(Context.User, cmd))
                        continue; //Don't include commands if user can't use them

                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description +=
                            $"{prefix}{cmd.Aliases.First()} {string.Join(", ", cmd.Parameters.Select(p => p.Name))} {Environment.NewLine}";
                }

                if (!string.IsNullOrWhiteSpace(description))
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = true;
                    });
            }

            if (Information.PmHelp)
                try
                {
                    var dm = await Context.User.GetOrCreateDMChannelAsync();
                    await dm.SendMessageAsync("", false, builder.Build());

                    if (Context.Channel is IGuildChannel _
                    ) //Only send "Check your DMs" if Message is in a Guild Channel
                        await ReplyAsync("Check your DMs!");
                } catch
                {
                    //could not send private
                    await ReplyAsync($"You're not allowing direct messages {Context.User.Mention}..",
                        embed: builder.Build());
                }
            else await ReplyAsync("", embed: builder.Build());
        }

        [Command("help")]
        [Summary("Show information and usage about a command")]
        public async Task HelpAsync([Summary("The command you want to see the documentation about")]
            string command)
        {
            var result = _service.Search(Context, command);
            string nl = Environment.NewLine;

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find the **{command}** command.");
                return;
            }

            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = $"Here's some Information about the **{command}** command:"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;
                bool multiple = cmd.Parameters.Count < 1;

                builder.AddField(x =>
                {
                    x.Name = $"{Information.Prefix}{cmd.Aliases.First()} {string.Join(" ", cmd.Parameters)}";
                    if (multiple) x.Value = $"Summary: {cmd.Summary}";
                    else
                        x.Value = $"Summary: {cmd.Summary}" + nl +
                                  $"Parameters: {nl}{string.Join($"{nl}", cmd.Parameters.Select(p => $"\t_{p.Name}_: {p.Summary}"))}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }


        public static bool CanUse(IUser user, CommandInfo command)
        {
            //if (user.Id == Information.OwnerId) {
            //    return true;
            //}

            var guilduser = user as IGuildUser;

            switch (command.Module.Name)
            {
                case "Admin":
                case "Clean":
                    if (guilduser == null) return false;
                    else return guilduser.GuildPermissions.Administrator;
                case "Owner":
                    if (guilduser == null) return false;
                    else return user.Id == Information.OwnerId;
                case "Votekick":
                    if (Information.AllowVotekick && guilduser != null) return true;
                    else return false;
                case "Xp":
                    if (guilduser != null)
                        if (GuildConfigManager.Get(guilduser.GuildId).EnableXpSystem)
                            return true;
                    return false;
                case "Profile":
                case "Hardware":
                case "Link":
                case "Code":
                    return guilduser != null;
            }

            return true;
        }
    }
}