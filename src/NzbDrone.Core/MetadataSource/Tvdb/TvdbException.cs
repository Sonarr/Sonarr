using System.Net;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.MetadataSource.Tvdb
{
    public class TvdbException : NzbDroneClientException
    {
        public TvdbException(string message) : base(HttpStatusCode.ServiceUnavailable, message)
        {
        }

        public TvdbException(string message, params object[] args) : base(HttpStatusCode.ServiceUnavailable, message, args)
        {
        }
    }
}
