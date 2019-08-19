using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace NzbDrone.Core.Diagnostics
{
    public enum ScriptDiagnosticSeverity
    {
        Info = 1,
        Warning = 2,
        Error = 3
    }

    public class ScriptDiagnostic
    {
        public int StartLineNumber { get; set; }
        public int StartColumn { get; set; }
        public int EndLineNumber { get; set; }
        public int EndColumn { get; set; }
        public string Message { get; set; }
        public ScriptDiagnosticSeverity Severity { get; set; }
        public string FullMessage { get; set; }

        public ScriptDiagnostic()
        {
        }

        public ScriptDiagnostic(Exception ex, int startLineNumber, int startColumn, int endLineNumber, int endColumn, string fullMessage)
        {
            StartLineNumber = startLineNumber;
            StartColumn = startColumn;
            EndLineNumber = endLineNumber;
            EndColumn = endColumn;
            Message = ex.Message;
            Severity = ScriptDiagnosticSeverity.Error;
            FullMessage = fullMessage;
        }

        public ScriptDiagnostic(Diagnostic diagnostic)
        {
            var lineSpan = diagnostic.Location.GetLineSpan();

            StartLineNumber = lineSpan.StartLinePosition.Line + 1;
            StartColumn = lineSpan.StartLinePosition.Character + 1;
            EndLineNumber = lineSpan.EndLinePosition.Line + 1;
            EndColumn = lineSpan.EndLinePosition.Character + 1;
            Message = diagnostic.GetMessage();
            Severity = (ScriptDiagnosticSeverity)diagnostic.Severity;
            FullMessage = diagnostic.ToString();
        }
    }
}
