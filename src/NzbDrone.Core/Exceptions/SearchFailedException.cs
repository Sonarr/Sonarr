using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Exceptions
{
    public class SearchFailedException : NzbDroneException
    {
        public SearchFailedException(string message)
            : base(message)
        {
        }
    }
}
