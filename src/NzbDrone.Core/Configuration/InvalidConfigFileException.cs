using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Configuration
{
    public class InvalidConfigFileException : NzbDroneException
    {
        public InvalidConfigFileException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
