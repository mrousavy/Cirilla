using System.Threading.Tasks;
using Cirilla.Services.GuildConfig;
using Cirilla.Services.Roslyn;
using Discord;
using Discord.Commands;

namespace Cirilla.Modules
{
    public class Code : ModuleBase
    {
        [Command("exec")]
        [Summary("Execute or run C# Code/Scripts")]
        public async Task Execute([Summary("The Code to execute")] [Remainder]
            string code)
        {
            try
            {
                //Requirements to execute C# Roslyn scripts
                if (!(Context.User is IGuildUser user) || !user.GuildPermissions.Speak)
                {
                    await ReplyAsync("You're not allowed to execute scripts!");
                    return;
                }

                if (!GuildConfigManager.Get(Context.Guild.Id).EnableScripts)
                {
                    await ReplyAsync($"C# Roslyn Scripts are disabled via Config. Ask {Information.Owner}.");
                    return;
                }

                //Handle exec async - so MessageReceived is not blocking
                var _ = HandleExecAsync(code, Context.Channel);
            } catch
            {
                await ReplyAsync("Something went wrong, sorry!");
            }
        }


        public async Task HandleExecAsync(string code, IMessageChannel contextChannel)
        {
            //Clean for code formatting
            string cleaned = code.Replace("```cs", string.Empty).Replace("`", string.Empty);

            var message = await ReplyAsync("Compiling.. :inbox_tray:");

            try
            {
                var embed = await ScriptRunnerService.ScriptEmbed(cleaned, Context.User, contextChannel);

                await message.ModifyAsync(props =>
                {
                    props.Content = string.Empty;
                    props.Embed = embed;
                });
            } catch (TaskCanceledException)
            {
                await message.ModifyAsync(props =>
                {
                    props.Content =
                        $"Sorry, the compilation took longer than expected! ({Information.CompileTimeout}ms)";
                });
            }
        }
    }
}