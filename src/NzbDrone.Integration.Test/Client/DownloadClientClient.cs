using System.Collections.Generic;
using Sonarr.Api.V3.DownloadClient;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class DownloadClientClient : ClientBase<DownloadClientResource>
    {
        public DownloadClientClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }

        public List<DownloadClientResource> Schema()
        {
            var request = BuildRequest("/schema");
            return Get<List<DownloadClientResource>>(request);
        }
    }
}