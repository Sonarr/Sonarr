using System.Net;

namespace Workarr.Http
{
    public class TlsFailureException : WebException
    {
        public TlsFailureException(WebRequest request, WebException innerException)
            : base("Failed to establish secure https connection to '" + request.RequestUri + "'.", innerException, WebExceptionStatus.SecureChannelFailure, innerException.Response)
        {
        }
    }
}
