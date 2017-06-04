using System;

namespace NzbDrone.Core.Download.Clients
{
    public class DownloadClientUnavailableException : DownloadClientException
    {
        public DownloadClientUnavailableException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }

        public DownloadClientUnavailableException(string message)
            : base(message)
        {
        }

        public DownloadClientUnavailableException(string message, Exception innerException, params object[] args)
            : base(string.Format(message, args), innerException)
        {
        }

        public DownloadClientUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
