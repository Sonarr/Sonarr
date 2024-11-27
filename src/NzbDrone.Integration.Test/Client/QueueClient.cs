using RestSharp;
using Sonarr.Api.V3.Queue;

namespace NzbDrone.Integration.Test.Client
{
    public class QueueClient : ClientBase<QueueResource>
    {
        public QueueClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }
    }
}
