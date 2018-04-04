using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

namespace Cirilla.Services.Roslyn
{
    public static class ScriptRunnerService
    {
        private static readonly string[] DefaultImports =
        {
            "System",
            "System.IO",
            "System.Linq",
            "System.Collections.Generic",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Net",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Net.Http"
        };

        private static readonly Assembly[] DefaultReferences =
        {
            typeof(Enumerable).GetTypeInfo().Assembly,
            typeof(List<string>).GetTypeInfo().Assembly,
            typeof(JsonConvert).GetTypeInfo().Assembly,
            typeof(string).GetTypeInfo().Assembly,
            typeof(ValueTuple).GetTypeInfo().Assembly,
            typeof(HttpClient).GetTypeInfo().Assembly
        };

        private static readonly ScriptOptions Options =
            ScriptOptions.Default
                .WithImports(DefaultImports)
                .WithReferences(DefaultReferences);

        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new BlacklistedTypesAnalyzer());

        private static readonly Random Random = new Random();


        /// <summary>
        ///     Run a Script from a string with the Roslyn compiler and return results as an Embed
        /// </summary>
        /// <param name="code">The C# script to run</param>
        /// <param name="user">The user for referencing in the Embed</param>
        /// <param name="contextChannel">The channel for referencing in the Code</param>
        /// <returns>The Embed with detailed information of the results</returns>
        public static async Task<Embed> ScriptEmbed(string code, IUser user, IMessageChannel contextChannel)
        {
            string nl = Environment.NewLine;
            // compilation
            var compileResult = await Compile(code);

            if (compileResult.CompileException != null)
            {
                ConsoleHelper.Log(
                    $"Error compiling C# script from {Helper.GetName(user)}! ({compileResult.CompileException.Message})",
                    LogSeverity.Info);
                return CompileError(user, code, compileResult);
            }

            var execResult = Execute(compileResult, contextChannel);

            if (execResult.ExecException != null)
            {
                ConsoleHelper.Log(
                    $"Error running C# script from {Helper.GetName(user)}! ({execResult.ExecException.Message})",
                    LogSeverity.Info);
                return RunError(user, code, execResult);
            }

            ConsoleHelper.Log($"{Helper.GetName(user)} ran a Roslyn script:{nl}{nl}{code}{nl}",
                LogSeverity.Info);
            return ScriptSuccess(user, code, execResult);
        }


        public static async Task<CompileResult> Compile(string code)
        {
            var result = new CompileResult();

            var compileSw = Stopwatch.StartNew();

            try
            {
                var compileCts = new CancellationTokenSource(Information.CompileTimeout);
                Script<object> script = CSharpScript.Create(code, Options, typeof(Globals));
                var compilation = script.GetCompilation()
                    .WithAnalyzers(Analyzers, cancellationToken: compileCts.Token);
                ImmutableArray<Diagnostic> compileDiagnostics =
                    await compilation.GetAllDiagnosticsAsync(compileCts.Token);
                ImmutableArray<Diagnostic> compileErrors = compileDiagnostics
                    .Where(a => a.Severity == DiagnosticSeverity.Error)
                    .ToImmutableArray();

                compileSw.Stop();
                long compileTime = compileSw.ElapsedMilliseconds;

                string diagnostics = Enumerable.Aggregate(compileDiagnostics, string.Empty,
                    (current, diagnostic) => current + diagnostic.ToString() + Environment.NewLine);

                if (compileErrors.Length > 0)
                    result.CompileException =
                        new CompilationErrorException(string.Join("\n", compileErrors.Select(a => a.GetMessage())),
                            compileErrors);

                result.Script = script;
                result.CompileDiagnostics = compileDiagnostics;
                result.CompileErrors = compileErrors;
                result.CompileTime = compileTime;
            } catch (Exception ex)
            {
                result.CompileException = ex;
            }

            return result;
        }


        public static ExecuteResult Execute(CompileResult compileResult, IMessageChannel contextChannel)
        {
            var stringBuilder = new StringBuilder();
            var result = new ExecuteResult
            {
                CompileResult = compileResult
            };
            var globals = new Globals
            {
                Console = new StringWriter(stringBuilder),
                Random = Random,
                ReplyAsync = async m => await contextChannel.SendMessageAsync(m)
            };

            var execSw = Stopwatch.StartNew();
            ScriptState<object> scriptState = null;
            var execCts = new CancellationTokenSource(Information.ExecutionTimeout + 100);

            try
            {
                var runThread = new Thread(async () =>
                {
                    scriptState = await compileResult.Script.RunAsync(globals, ex => true, execCts.Token);
                });
                runThread.Start();
                bool successfullyEnded = runThread.Join(Information.ExecutionTimeout);

                if (!successfullyEnded) throw new TaskCanceledException();

                execSw.Stop();
                result.ExecuteTime = execSw.ElapsedMilliseconds;

                if (scriptState != null)
                {
                    if (scriptState.Exception != null) result.ExecException = scriptState.Exception;

                    result.ReturnValue = scriptState.ReturnValue;
                }

                if (stringBuilder.Length > 0)
                    result.ConsoleOutput = stringBuilder.Length > 1024
                        ? stringBuilder.ToString().Substring(0, 1019) + " [..]"
                        : stringBuilder.ToString();
            } catch (TaskCanceledException)
            {
                result.ExecException = new TimeoutException(
                    $"The execution of the script took longer than expected! (> {Information.ExecutionTimeout}ms)");
            } catch (Exception ex)
            {
                result.ExecException = ex;
            }

            return result;
        }


        public static Embed CompileError(IUser user, string code, CompileResult result)
        {
            var builder = DefaultEmbed();
            string nl = Environment.NewLine;

            builder.Color = new Color(180, 8, 8);
            builder.AddField("Requested by", user.Mention);
            builder.AddField("Result", "Failed (Compilation Error)");
            string codeTrim = code.Length > 1019
                ? code.Substring(0, 1019) + " [..]"
                : code;
            builder.AddField("Code", $"```cs{nl}{codeTrim}{nl}```");


            string exceptionTitle = $"{result.CompileException.GetType()}:";
            string exceptionContent = $"```accesslog{nl}{result.CompileException.Message}{nl}```";

            string exceptionTrim = exceptionContent.Length > 1019
                ? exceptionContent.Substring(0, 1019) + " [..]"
                : exceptionContent;

            builder.AddField(exceptionTitle, exceptionTrim);

            if (!string.IsNullOrWhiteSpace(result.CompileDiagnosticsString))
            {
                string diagnosticsTrim = result.CompileDiagnosticsString.Length > 1019
                    ? result.CompileDiagnosticsString.Substring(0, 1019) + " [..]"
                    : result.CompileDiagnosticsString;

                builder.AddField("Diagnostics", diagnosticsTrim);
            }

            builder.Footer = new EmbedFooterBuilder
            {
                Text = "Compile: / | Execute: /"
            };

            return builder.Build();
        }

        public static Embed RunError(IUser user, string code, ExecuteResult result)
        {
            var builder = DefaultEmbed();
            string nl = Environment.NewLine;

            builder.Color = new Color(180, 8, 8);
            builder.AddField("Requested by", user.Mention);
            builder.AddField("Result", "Failed (Execution Error)");
            string codeTrim = code.Length > 1019
                ? code.Substring(0, 1019) + " [..]"
                : code;
            builder.AddField("Code", $"```cs{nl}{codeTrim}{nl}```");


            string exceptionTitle = $"{result.ExecException.GetType()}:";
            string exceptionContent = $"```accesslog{nl}{result.ExecException.Message}{nl}```";

            string exceptionTrim = exceptionContent.Length > 1019
                ? exceptionContent.Substring(0, 1019) + " [..]"
                : exceptionContent;

            builder.AddField(exceptionTitle, exceptionTrim);

            if (!string.IsNullOrWhiteSpace(result.CompileResult.CompileDiagnosticsString))
            {
                string diagnosticsTrim = result.CompileResult.CompileDiagnosticsString.Length > 1024
                    ? result.CompileResult.CompileDiagnosticsString.Substring(0, 1019) + " [..]"
                    : result.CompileResult.CompileDiagnosticsString;

                builder.AddField("Diagnostics", diagnosticsTrim);
            }

            builder.Footer = new EmbedFooterBuilder
            {
                Text = $"Compile: {result.CompileResult.CompileTime}ms | Execute: /"
            };


            return builder.Build();
        }

        public static Embed ScriptSuccess(IUser user, string code, ExecuteResult result)
        {
            var builder = DefaultEmbed();
            string nl = Environment.NewLine;

            builder.Color = new Color(50, 155, 0);
            builder.AddField("Requested by", user.Mention);
            builder.AddField("Result", "Successful");
            string codeTrim = code.Length > 1019
                ? code.Substring(0, 1019) + " [..]"
                : code;
            builder.AddField("Code", $"```cs{nl}{codeTrim}{nl}```");

            if (result.ReturnValue == null)
            {
                if (string.IsNullOrWhiteSpace(result.ConsoleOutput))
                    builder.AddField("Result:  /", $"```accesslog{nl}(No value was returned){nl}```");
                else builder.AddField("Console Output:", $"```accesslog{nl}{result.ConsoleOutput}{nl}```");
            } else
            {
                string resultTrim = result.ReturnValue.ToString().Length > 1024
                    ? result.ReturnValue.ToString().Substring(0, 1019) + " [..]"
                    : result.ReturnValue.ToString();
                builder.AddField($"Result: {result.ReturnValue.GetType()}",
                    $"```cs{nl}{resultTrim}{nl}```");
            }

            if (!string.IsNullOrWhiteSpace(result.CompileResult.CompileDiagnosticsString))
                builder.AddField("Diagnostics",
                    result.CompileResult.CompileDiagnosticsString.Length > 1024
                        ? $"```{nl}{result.CompileResult.CompileDiagnosticsString.Substring(0, 1019)} [..]{nl}```"
                        : $"```{nl}{result.CompileResult.CompileDiagnosticsString}{nl}```");

            builder.Footer = new EmbedFooterBuilder
            {
                Text = $"Compile: {result.CompileResult.CompileTime}ms | Execute: {result.ExecuteTime}ms"
            };

            return builder.Build();
        }


        public static EmbedBuilder DefaultEmbed()
        {
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = "Roslyn Scripting",
                    IconUrl = "http://ourcodeworld.com/public-media/gallery/categorielogo-5713d627ccabf.png" //C# Icon
                }
            };

            return builder;
        }
    }


    public class CompileTimeoutException : Exception
    {
        public CompileTimeoutException(string message) : base(message)
        { }
    }
}