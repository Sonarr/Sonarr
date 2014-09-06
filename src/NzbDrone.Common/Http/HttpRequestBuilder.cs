using System;

namespace NzbDrone.Common.Http
{
    public class HttpRequestBuilder
    {
        public Uri BaseUri { get; private set; }

        public Action<HttpRequest> PostProcess { get; set; }

        public HttpRequestBuilder(string baseUri)
        {
            BaseUri = new Uri(baseUri);
        }

        public virtual HttpRequest Build(string path)
        {
            if (BaseUri.ToString().EndsWith("/"))
            {
                path = path.TrimStart('/');
            }

            var request = new HttpRequest(BaseUri + path);

            if (PostProcess != null)
            {
                PostProcess(request);
            }

            return request;
        }
    }
}