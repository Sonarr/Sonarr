using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Download.Clients
{
    public class DownloadClientConnectionException : DownloadClientException
    {
        public DownloadClientConnectionException(string message, params object[] args)
            : base(message, args)
        {

        }

        public DownloadClientConnectionException(string message)
            : base(message)
        {

        }

        public DownloadClientConnectionException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {

        }

        public DownloadClientConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
