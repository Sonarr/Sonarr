using System;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Indexers
{
    public class IndexerRequest
    {
        public HttpRequest HttpRequest { get; private set; }

        public IndexerRequest(String url, HttpAccept httpAccept)
        {
            HttpRequest = new HttpRequest(url, httpAccept);
        }

        public IndexerRequest(HttpRequest httpRequest)
        {
            HttpRequest = httpRequest;
        }

        public Uri Url
        {
            get { return HttpRequest.Url; }
        }
    }
}
