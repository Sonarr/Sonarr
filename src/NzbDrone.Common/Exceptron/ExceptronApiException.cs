using System;
using System.Net;

namespace NzbDrone.Common.Exceptron
{
    public class ExceptronApiException : Exception
    {
        public ExceptronApiException(WebException innerException, string message)
            : base(message, innerException)
        {
            Response = (HttpWebResponse)innerException.Response;
        }

        public HttpWebResponse Response { get; private set; }
    }
}
