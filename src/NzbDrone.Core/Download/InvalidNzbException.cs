using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Download
{
    public class InvalidNzbException : NzbDroneException
    {
        public InvalidNzbException(string message, params object[] args)
            : base(message, args)
        {
        }

        public InvalidNzbException(string message)
            : base(message)
        {
        }

        public InvalidNzbException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }

        public InvalidNzbException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
