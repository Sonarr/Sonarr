using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;

namespace NzbDrone.Api.Helpers
{
    public static class HttpRequestExtensions
    {
        public static string GetApiKey(this IHttpRequest httpReq)
        {
            var auth = httpReq.Headers[HttpHeaders.Authorization];
            if (auth == null) return null;

            var split = auth.Split(' ');

            if (split.Count() != 2)
                return null;

            if (!split[0].Equals("APIKEY", StringComparison.InvariantCultureIgnoreCase))
                return null;

            return split[1];
        }
    }
}
