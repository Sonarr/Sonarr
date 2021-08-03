using System;

namespace NzbDrone.Core.Download.Clients
{
    public class DownloadClientAuthenticationException : DownloadClientException
    {
        public DownloadClientAuthenticationException(string message, params object[] args)
            : base(message, args)
        {
        }

        public DownloadClientAuthenticationException(string message)
            : base(message)
        {
        }

        public DownloadClientAuthenticationException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }

        public DownloadClientAuthenticationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
