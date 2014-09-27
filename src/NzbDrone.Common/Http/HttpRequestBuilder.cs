using System;
using System.Net;

namespace NzbDrone.Common.Http
{
    public class HttpRequestBuilder
    {
        public Uri BaseUri { get; private set; }
        public bool SupressHttpError { get; set; }
        public NetworkCredential NetworkCredential { get; set; }

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

            var request = new HttpRequest(BaseUri + path)
            {
                SuppressHttpError = SupressHttpError,
                NetworkCredential = NetworkCredential
            };

            if (PostProcess != null)
            {
                PostProcess(request);
            }

            return request;
        }
    }
}