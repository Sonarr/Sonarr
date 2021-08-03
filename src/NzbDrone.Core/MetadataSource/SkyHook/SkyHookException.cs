using System;
using System.Net;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SkyHookException : NzbDroneClientException
    {
        public SkyHookException(string message)
            : base(HttpStatusCode.ServiceUnavailable, message)
        {
        }

        public SkyHookException(string message, params object[] args)
            : base(HttpStatusCode.ServiceUnavailable, message, args)
        {
        }

        public SkyHookException(string message, Exception innerException, params object[] args)
            : base(HttpStatusCode.ServiceUnavailable, message, innerException, args)
        {
        }
    }
}
