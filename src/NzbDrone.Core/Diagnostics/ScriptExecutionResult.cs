using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Diagnostics
{
    public class ScriptExecutionResult
    {
        public string StateId { get; set; }

        public Exception Exception { get; set; }
        public object ReturnValue { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public ScriptValidationResult Validation { get; set; }
    }
}
