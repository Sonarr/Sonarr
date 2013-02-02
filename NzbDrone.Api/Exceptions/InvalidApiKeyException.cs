using System;
using System.Linq;

namespace NzbDrone.Api.Exceptions
{
    [Serializable]
    public class InvalidApiKeyException : Exception
    {
        public InvalidApiKeyException()
        {
        }

        public InvalidApiKeyException(string message) : base(message)
        {
        }
    }
}
