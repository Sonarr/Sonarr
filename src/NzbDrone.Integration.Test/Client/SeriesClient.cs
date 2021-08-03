using System.Collections.Generic;
using System.Net;
using RestSharp;
using Sonarr.Api.V3.Series;

namespace NzbDrone.Integration.Test.Client
{
    public class SeriesClient : ClientBase<SeriesResource>
    {
        public SeriesClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }

        public List<SeriesResource> Lookup(string term)
        {
            var request = BuildRequest("lookup");
            request.AddQueryParameter("term", term);
            return Get<List<SeriesResource>>(request);
        }

        public List<SeriesResource> Editor(List<SeriesResource> series)
        {
            var request = BuildRequest("editor");
            request.AddJsonBody(series);
            return Put<List<SeriesResource>>(request);
        }

        public SeriesResource Get(string slug, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var request = BuildRequest(slug);
            return Get<SeriesResource>(request, statusCode);
        }
    }

    public class SystemInfoClient : ClientBase<SeriesResource>
    {
        public SystemInfoClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }
    }
}
