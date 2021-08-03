using System.Collections.Generic;
using RestSharp;
using Sonarr.Api.V3.Indexers;

namespace NzbDrone.Integration.Test.Client
{
    public class IndexerClient : ClientBase<IndexerResource>
    {
        public IndexerClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }

        public List<IndexerResource> Schema()
        {
            var request = BuildRequest("/schema");
            return Get<List<IndexerResource>>(request);
        }
    }
}
