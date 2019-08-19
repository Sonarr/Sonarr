using System.Linq;
using Microsoft.CodeAnalysis;

namespace NzbDrone.Core.Diagnostics
{
    public class ScriptValidationResult
    {
        public ScriptDiagnostic[] Messages { get; set; }

        public bool HasWarnings => Messages.Any(v => v.Severity == ScriptDiagnosticSeverity.Warning);
        public bool HasErrors => Messages.Any(v => v.Severity == ScriptDiagnosticSeverity.Error);

    }
}
