using System;

namespace NzbDrone.Core.Parser
{
    public class LanguageParsingException : Exception
    {
        public LanguageParsingException(string message)
            : base(message)
        {
        }
    }
}
