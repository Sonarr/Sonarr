using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
