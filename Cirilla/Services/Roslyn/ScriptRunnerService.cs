﻿using Discord;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
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

namespace Cirilla.Services.Roslyn {
    public static class ScriptRunnerService {
        private static readonly string[] DefaultImports = {
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

        private static readonly Assembly[] DefaultReferences = {
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
        /// Run a Script from a string with the Roslyn compiler and return results as an Embed
        /// </summary>
        /// <param name="code">The C# script to run</param>
        /// <param name="user">The user for referencing in the Embed</param>
        /// <param name="contextChannel">The channel for referencing in the Code</param>
        /// <returns>The Embed with detailed information of the results</returns>
        public static async Task<Embed> ScriptEmbed(string code, IUser user, IMessageChannel contextChannel) {
            string nl = Environment.NewLine;

            EmbedBuilder builder = new EmbedBuilder {
                Author = new EmbedAuthorBuilder {
                    Name = "Roslyn Scripting",
                    IconUrl = "http://ourcodeworld.com/public-media/gallery/categorielogo-5713d627ccabf.png" //C# Icon
                }
            };

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter textWriter = new StringWriter(stringBuilder);
            bool successful;
            long compileTime, execTime;
            CompilationErrorException compileException = null;
            Exception runEx = null;

            Stopwatch compileSw = Stopwatch.StartNew();

            // compilation
            CancellationTokenSource compileCts = new CancellationTokenSource(Information.CompileTimeout);
            Script<object> script = CSharpScript.Create(code, Options, typeof(Globals));
            CompilationWithAnalyzers compilation = script.GetCompilation().WithAnalyzers(Analyzers, cancellationToken: compileCts.Token);
            ImmutableArray<Diagnostic> compileResult = await compilation.GetAllDiagnosticsAsync(compileCts.Token);
            ImmutableArray<Diagnostic> compileErrors = compileResult.Where(a => a.Severity == DiagnosticSeverity.Error)
                .ToImmutableArray();

            compileSw.Stop();
            compileTime = compileSw.ElapsedMilliseconds;
            string diagnostics = Enumerable.Aggregate(compileResult, string.Empty,
                (current, diagnostic) => current + diagnostic.ToString());

            if (compileErrors.Length > 0) {
                compileException =
                    new CompilationErrorException(string.Join("\n", compileErrors.Select(a => a.GetMessage())),
                        compileErrors);
            }

            Globals globals = new Globals {
                Console = textWriter,
                Random = Random,
                Client = Cirilla.Client,
                ReplyAsync = async m => await contextChannel.SendMessageAsync(m)
            };

            Stopwatch execSw = Stopwatch.StartNew();
            ScriptState<object> result = null;
            //execute
            CancellationTokenSource execCts = new CancellationTokenSource(Information.ExecutionTimeout);
            try {
                result = await script.RunAsync(globals, ex => true, execCts.Token);

                if (result.Exception == null) {
                    successful = true;
                } else {
                    runEx = result.Exception;
                    successful = false;
                }
            } catch (TaskCanceledException) {
                runEx = new TimeoutException($"The execution of the script took longer than expected! ({Information.ExecutionTimeout}ms)");
                successful = false;
            } catch (Exception ex) {
                runEx = ex;
                successful = false;
            }
            execSw.Stop();
            execTime = execSw.ElapsedMilliseconds;

            if (successful) {
                await ConsoleHelper.Log($"{Helper.GetName(user)} ran a Roslyn script:{nl}{nl}{code}{nl}",
                    LogSeverity.Info);

                builder.Color = new Color(50, 155, 0);
                builder.AddField("Requested by", user.Mention);
                builder.AddField("Result", "Successful");
                string codeTrim = code.Length > 1024 ? code.Substring(0, 1024) : code;
                builder.AddField("Code", $"```cs{nl}{code}{nl}```");
                if (result.ReturnValue == null) {
                    if (string.IsNullOrWhiteSpace(stringBuilder.ToString())) {
                        builder.AddField("Result:  /", $"```accesslog{nl}(No value was returned){nl}```");
                    } else {
                        string resultTrim = stringBuilder.ToString().Length > 1024 ? stringBuilder.ToString().Substring(0, 1024) : stringBuilder.ToString();
                        builder.AddField("Console Output:", $"```accesslog{nl}{resultTrim}{nl}```");
                    }
                } else {
                    string resultTrim = result.ReturnValue.ToString().Length > 1024 ? result.ReturnValue.ToString().Substring(0, 1024) : result.ReturnValue.ToString();
                    builder.AddField($"Result: {result.ReturnValue.GetType()}",
                        $"```cs{nl}{resultTrim}{nl}```");
                }
                if (!string.IsNullOrWhiteSpace(diagnostics)) {
                    builder.AddField("Diagnostics",
                        diagnostics.Length > 1024
                            ? $"```{nl}{diagnostics.Substring(0, 1024)} [...]{nl}```"
                            : $"```{nl}{diagnostics}{nl}```");
                }
            } else {
                await ConsoleHelper.Log(
                    $"Error compiling C# script from {Helper.GetName(user)}! ({compileException?.Message})",
                    LogSeverity.Info);

                builder.Color = new Color(180, 8, 8);
                builder.AddField("Requested by", user.Mention);
                builder.AddField("Result", "Failed");
                string codeTrim = code.Length > 1024 ? code.Substring(0, 1024) : code;
                builder.AddField("Code", $"```cs{nl}{codeTrim}{nl}```");

                string exceptionTitle = compileException == null ? $"Exception: {runEx.GetType()}" : "Compiler Error:";
                string exceptionContent = compileException == null
                    ? $"```accesslog{nl}{runEx.Message}{nl}```"
                    : $"```accesslog{nl}{compileException.Message}{nl}```";
                string exceptionTrim = exceptionContent.Length > 1024 ? exceptionContent.Substring(0, 1024) : exceptionContent;
                builder.AddField(exceptionTitle, exceptionTrim);
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