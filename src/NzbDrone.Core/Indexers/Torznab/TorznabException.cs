using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Indexers.Torznab
{
    public class TorznabException : NzbDroneException
    {
        public TorznabException(string message, params object[] args)
            : base(message, args)
        {
        }

        public TorznabException(string message)
            : base(message)
        {
        }
    }
}
