﻿using Workarr.Http;

namespace Workarr.Indexers
{
    public class IndexerRequest
    {
        public HttpRequest HttpRequest { get; private set; }

        public IndexerRequest(string url, HttpAccept httpAccept)
        {
            HttpRequest = new HttpRequest(url, httpAccept);
        }

        public IndexerRequest(HttpRequest httpRequest)
        {
            HttpRequest = httpRequest;
        }

        public HttpUri Url => HttpRequest.Url;
    }
}
