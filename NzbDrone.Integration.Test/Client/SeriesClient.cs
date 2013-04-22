using System.Collections.Generic;
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

    }
}
