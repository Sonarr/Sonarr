using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Configuration
{
    public class InvalidConfigFileException : Exception
    {
        public InvalidConfigFileException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
