using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

namespace Cirilla.Services.Roslyn
{
    public struct CompileResult
    {
        public ImmutableArray<Diagnostic> CompileDiagnostics { get; set; }
        public string CompileDiagnosticsString { get; set; }
        public ImmutableArray<Diagnostic> CompileErrors { get; set; }
        public Exception CompileException { get; set; }
        public Script<object> Script { get; set; }

        public long CompileTime { get; set; }
    }
}