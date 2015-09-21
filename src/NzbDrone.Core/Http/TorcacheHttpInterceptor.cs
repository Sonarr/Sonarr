using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Http;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Http
{
    public class TorCacheHttpRequestInterceptor : IHttpRequestInterceptor
    {
        public HttpRequest PreRequest(HttpRequest request)
        {
            if (request.Url.Host == "torcache.net" && request.UriBuilder.Query.IsNotNullOrWhiteSpace())
            {
                request.UriBuilder.Query = string.Empty;
            }

            return request;
        }

        public HttpResponse PostResponse(HttpResponse response)
        {
            return response;
        }
    }
}
