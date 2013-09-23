using System.Net;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.MetadataSource.Trakt
{
    public class TraktException : NzbDroneClientException
    {
        public TraktException(string message) : base(HttpStatusCode.ServiceUnavailable, message)
        {
        }

        public TraktException(string message, params object[] args) : base(HttpStatusCode.ServiceUnavailable, message, args)
        {
        }
    }
}
