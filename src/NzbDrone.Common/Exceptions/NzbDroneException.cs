using System;

namespace NzbDrone.Common.Exceptions
{
    public abstract class NzbDroneException : ApplicationException
    {
        protected NzbDroneException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }

        protected NzbDroneException(string message)
            : base(message)
        {
        }

        protected NzbDroneException(string message, Exception innerException, params object[] args)
            : base(string.Format(message, args), innerException)
        {
        }

        protected NzbDroneException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
