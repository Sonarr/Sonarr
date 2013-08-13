using System.Collections.Generic;
using System.Net;
using NzbDrone.Api.Series;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class SeriesClient : ClientBase<SeriesResource>
    {
        public SeriesClient(IRestClient restClient)
            : base(restClient)
        {
        }

        public List<SeriesResource> Lookup(string term)
        {
            var request = BuildRequest("lookup?term={term}");
            request.AddUrlSegment("term", term);
            return Get<List<SeriesResource>>(request);
        }

        public SeriesResource Get(string slug, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var request = BuildRequest(slug);
            return Get<SeriesResource>(request, statusCode);
        }

    }


    public class SystemInfoClient : ClientBase<SeriesResource>
    {
        public SystemInfoClient(IRestClient restClient)
            : base(restClient)
        {
        }


    }
}
