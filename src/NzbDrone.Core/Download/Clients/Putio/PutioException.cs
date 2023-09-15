using System;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public class PutioException : DownloadClientException
    {
        public PutioException(string message)
            : base(message)
        {

        }
    }
}
