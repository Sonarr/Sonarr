using NzbDrone.Common.Http;

namespace NzbDrone.Core.Http
{
    public class TorCacheHttpRequestInterceptor : IHttpRequestInterceptor
    {
        public HttpRequest PreRequest(HttpRequest request)
        {
            // torcache behaves strangely when it has query params and/or no Referer or browser User-Agent.
            // It's a bit vague, and we don't need the query params. So we remove the query params and set a Referer to be safe.
            if (request.Url.Host == "torcache.net")
            {
                request.Url = request.Url.SetQuery(string.Empty);
                request.Headers.Add("Referer", request.Url.Scheme + @"://torcache.net/");
            }

            return request;
        }

        public HttpResponse PostResponse(HttpResponse response)
        {
            return response;
        }
    }
}
