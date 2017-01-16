using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Indexers.Exceptions
{
    public class UnsupportedFeedException : NzbDroneException
    {
        public UnsupportedFeedException(string message, params object[] args)
            : base(message, args)
        {
        }

        public UnsupportedFeedException(string message)
            : base(message)
        {
        }
    }
}
