using Discord;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Cirilla.Services.Roslyn {
    public static class ScriptRunnerService {

        /// <summary>
        /// Run a Script from a string with the Roslyn compiler and return results as an Embed
        /// </summary>
        /// <param name="code">The C# script to run</param>
        /// <param name="user">The user for referencing in the Embed</param>
        /// <returns>The Embed with detailed information of the results</returns>
        public static async Task<Embed> ScriptEmbed(string code, IUser user) {
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

            CancellationTokenSource compileCancellation = null, execCancellation = null;
            try {

                ScriptOptions options = ScriptOptions.Default;
                options.AddImports("System");
                options.AddImports("System.Threading");
                options.AddImports("System.Threading.Tasks");
                options.AddImports("System.Math");

                Script script = CSharpScript.Create(code, options);

                //Compile script
                compileSw.Start();
                compileCancellation = new CancellationTokenSource(Information.CompileTimeout);
                ImmutableArray<Diagnostic> diagnostics = script.Compile(compileCancellation.Token);
                Compilation compilation = script.GetCompilation();
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
                execCancellation = new CancellationTokenSource(Information.ExecutionTimeout);
                ScriptState state = await script.RunAsync(null, execCancellation.Token);
                result = state.ReturnValue;
                execSw.Stop();
                execTime = execSw.ElapsedMilliseconds;

                successful = true;
            } catch (CompilationErrorException ex) {
                exception = ex;
                compiled = false;
                compileTime = -1;
            } catch (TaskCanceledException ex) {
                if (ex.CancellationToken == compileCancellation.Token) {
                    compiled = false;
                    compileTime = -1;
                    exception = new CompileTimeoutException($"Compilation took longer than {Information.CompileTimeout}ms!");
                } else {
                    compiled = true;
                    execTime = -1;
                    exception = new CompileTimeoutException($"Execution took longer than {Information.ExecutionTimeout}ms!");
                }
            } catch (Exception ex) {
                exception = ex;
            }

            if (successful) {
                await ConsoleHelper.Log($"{Helper.GetName(user)} ran a Roslyn script:{nl}{nl}{code}{nl}", LogSeverity.Info);

                builder.Color = new Color(50, 155, 0);
                builder.AddField("Requested by", user.Mention);
                builder.AddField("Result", "Successful");
                builder.AddField("Code", $"```cs{nl}{code}{nl}```");
                if (result == null) {
                    builder.AddField($"Result:  /", $"```accesslog{nl}(No value was returned){nl}```");
                } else {
                    builder.AddField($"Result: {result.GetType()}", $"```cs{nl}{result}{nl}```");
                }
            } else {
                await ConsoleHelper.Log($"Error compiling C# script from {Helper.GetName(user)}! ({exception.Message})", LogSeverity.Info);

                builder.Color = new Color(180, 8, 8);
                builder.AddField("Requested by", user.Mention);
                builder.AddField("Result", "Failed");
                builder.AddField("Code", $"```cs{nl}{code}{nl}```");
                if (compiled) {
                    //Exception at runtime
                    builder.AddField($"Exception: {exception.GetType()}", $"```accesslog{nl}{exception.Message}{nl}```");
                } else {
                    //Exception at compilation
                    builder.AddField("Compiler Error:", $"```accesslog{nl}{exception.Message}{nl}```");
                }
            }

            string compileTimeStr = compileTime == -1 ? "/" : compileTime + "ms";
            string execTimeStr = execTime == -1 ? "/" : execTime + "ms";
            builder.Footer = new EmbedFooterBuilder {
                Text = $"Compilation: {compileTimeStr} | Execution: {execTimeStr}"
            };

            return builder.Build();
        }
    }



    public class CompileTimeoutException : Exception {
        public CompileTimeoutException(string message) : base(message) { }
    }
}
