using NzbDrone.Api.Indexers;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class ReleaseClient : ClientBase<ReleaseResource>
    {
        public ReleaseClient(IRestClient restClient)
            : base(restClient)
        {
        }



    }
}
