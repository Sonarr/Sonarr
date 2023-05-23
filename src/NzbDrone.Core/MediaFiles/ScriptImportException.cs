using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.MediaFiles
{
    public class ScriptImportException : NzbDroneException
    {
        public ScriptImportException(string message)
            : base(message)
        {
        }

        public ScriptImportException(string message, params object[] args)
            : base(message, args)
        {
        }

        public ScriptImportException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
