using System;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Indexers
{
    public class IndexerResponse
    {
        private readonly IndexerRequest _indexerRequest;
        private readonly HttpResponse _httpResponse;

        public IndexerResponse(IndexerRequest indexerRequest, HttpResponse httpResponse)
        {
            _indexerRequest = indexerRequest;
            _httpResponse = httpResponse;
        }

        public IndexerRequest Request
        {
            get { return _indexerRequest; }
        }

        public HttpRequest HttpRequest
        {
            get { return _httpResponse.Request; }
        }

        public HttpResponse HttpResponse
        {
            get { return _httpResponse; }
        }

        public String Content
        {
            get { return _httpResponse.Content; }
        }
    }
}
