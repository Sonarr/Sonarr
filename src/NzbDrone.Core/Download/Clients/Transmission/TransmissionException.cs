using System;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public class TransmissionException : DownloadClientException
    {
        public TransmissionException(String message)
            : base(message)
        {

        }
    }
}
