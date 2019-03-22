using System;

namespace Sonarr.Http.Exceptions
{
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
