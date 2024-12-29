using Workarr.Exceptions;

namespace Workarr.MediaFiles
{
    public class ScriptImportException : WorkarrException
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
