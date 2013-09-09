using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NzbDrone.Common.Exceptions;

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
