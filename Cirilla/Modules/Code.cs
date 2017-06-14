using Discord;
using Discord.Commands;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Code : ModuleBase {
        [Command("exec"), Summary("Execute or run C# Code/Scripts")]
        public async Task Execute([Summary("The Code to execute")] [Remainder] string code) {
            //Requirements to execute C# Roslyn scripts
            if (!(Context.User is IGuildUser user) || !user.GuildPermissions.KickMembers) {
                await ReplyAsync("You're not allowed to use this super premium command!");
                return;
            }

            string nl = Environment.NewLine;

            EmbedBuilder builder = new EmbedBuilder {
                Author = new EmbedAuthorBuilder {
                    Name = $"Roslyn Scripting",
                    IconUrl = "http://ourcodeworld.com/public-media/gallery/categorielogo-5713d627ccabf.png" //C# Icon
                }
            };

            object result = null;
            Exception exception = null;
            bool successful = false;
            bool compiled = false;
            long compileTime = -1, execTime = -1;
            Stopwatch compileSw = new Stopwatch();
            Stopwatch execSw = new Stopwatch();

            try {

                ScriptOptions options = ScriptOptions.Default;
                options.AddImports("System");
                options.AddImports("System.Threading");
                options.AddImports("System.Threading.Tasks");
                options.AddImports("System.Math");

                Script script = CSharpScript.Create(code, options);

                //Compile script
                compileSw.Start();
                CancellationTokenSource compileCancellation = new CancellationTokenSource(Information.CompileTimeout);
                ImmutableArray<Diagnostic> diagnostics = script.Compile(compileCancellation.Token);
                string diagnosticResult = "";
                foreach (Diagnostic diagnostic in diagnostics) {
                    diagnosticResult += diagnostic.Descriptor + nl;
                }
                if (!string.IsNullOrWhiteSpace(diagnosticResult)) {
                    await ConsoleHelper.Log(diagnosticResult, LogSeverity.Debug);
                }
                compileSw.Stop();
                compileTime = compileSw.ElapsedMilliseconds;
                compiled = true;

                //Run script
                execSw.Start();
                CancellationTokenSource execCancellation = new CancellationTokenSource(Information.ExecutionTimeout);
                ScriptState state = await script.RunAsync(null, execCancellation.Token);
                result = state.ReturnValue;
                execSw.Stop();
                execTime = execSw.ElapsedMilliseconds;

                successful = true;
            } catch (CompilationErrorException ex) {
                exception = ex;
            } catch (TaskCanceledException) {
                exception = new CompileTimeoutException($"Compilation took longer than {Information.CompileTimeout}ms!");
            } catch (Exception ex) {
                exception = ex;
            }

            if (successful) {
                await ConsoleHelper.Log($"{Helper.GetName(Context.User)} ran a Roslyn script:{nl}{nl}{code}{nl}", LogSeverity.Info);

                builder.Color = new Color(0, 255, 0);
                builder.AddField("Requested by", Context.User.Mention);
                builder.AddField("Result", "Successful");
                builder.AddField("Code", $"```cs{nl}{code}{nl}```");
                if (result == null) {
                    builder.AddField($"Result:  /", $"```accesslog{nl}(No value was returned){nl}```");
                } else {
                    builder.AddField($"Result: {result.GetType()}", $"```cs{nl}{result}{nl}```");
                }
                builder.Footer = new EmbedFooterBuilder {
                    Text = $"Compilation: {compileTime}ms | Execution: {execTime}ms"
                };
            } else {
                await ConsoleHelper.Log($"Error compiling C# script from {Helper.GetName(Context.User)}! ({exception.Message})", LogSeverity.Info);

                builder.Color = new Color(255, 0, 0);
                builder.AddField("Requested by", Context.User.Mention);
                builder.AddField("Result", "Failed");
                builder.AddField("Code", $"```cs{nl}{code}{nl}```");
                if (compiled) {
                    //Exception at runtime
                    builder.AddField($"Exception: {exception.GetType()}", $"```accesslog{nl}{exception.Message}{nl}```");
                } else {
                    //Exception at compilation
                    builder.AddField("Compiler Error:", $"```accesslog{nl}{exception.Message}{nl}```");
                }
                builder.Footer = new EmbedFooterBuilder {
                    Text = $"Compilation: {compileTime}ms | Execution: {execTime}ms"
                };
            }

            await ReplyAsync("", embed: builder.Build());
        }
    }



    public class CompileTimeoutException : Exception {
        public CompileTimeoutException(string message) : base(message) { }
    }
}
