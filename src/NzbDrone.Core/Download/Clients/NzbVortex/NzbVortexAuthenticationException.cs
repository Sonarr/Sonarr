using System;

namespace NzbDrone.Core.Download.Clients.NzbVortex
{
    public class NzbVortexAuthenticationException : DownloadClientException
    {
        public NzbVortexAuthenticationException(string message, params object[] args)
            : base(message, args)
        {
        }

        public NzbVortexAuthenticationException(string message)
            : base(message)
        {
        }

        public NzbVortexAuthenticationException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }

        public NzbVortexAuthenticationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
