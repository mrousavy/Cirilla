using System;

namespace Cirilla.Services.Roslyn
{
    public struct ExecuteResult
    {
        public CompileResult CompileResult { get; set; }
        public Exception ExecException { get; set; }
        public object ReturnValue { get; set; }
        public string ConsoleOutput { get; set; }

        public long ExecuteTime { get; set; }
    }
}