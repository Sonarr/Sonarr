using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Indexers.Exceptions
{
    public class ApiKeyException : NzbDroneException
    {
        public ApiKeyException(string message, params object[] args)
            : base(message, args)
        {
        }

        public ApiKeyException(string message)
            : base(message)
        {
        }
    }
}
