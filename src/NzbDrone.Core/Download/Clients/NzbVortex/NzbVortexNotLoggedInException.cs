using System;

namespace NzbDrone.Core.Download.Clients.NzbVortex
{
    public class NzbVortexNotLoggedInException : DownloadClientException
    {
        public NzbVortexNotLoggedInException()
            : this("Authentication is required")
        {
        }

        public NzbVortexNotLoggedInException(string message, params object[] args)
            : base(message, args)
        {
        }

        public NzbVortexNotLoggedInException(string message)
            : base(message)
        {
        }

        public NzbVortexNotLoggedInException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }

        public NzbVortexNotLoggedInException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
