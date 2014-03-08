using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Download.Clients
{
    public class DownloadClientException : NzbDroneException
    {
        public DownloadClientException(string message, params object[] args) : base(message, args)
        {
        }

        public DownloadClientException(string message) : base(message)
        {
        }
    }
}
