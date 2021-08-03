using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Configuration
{
    public class InvalidConfigFileException : NzbDroneException
    {
        public InvalidConfigFileException(string message)
            : base(message)
        {
        }

        public InvalidConfigFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
