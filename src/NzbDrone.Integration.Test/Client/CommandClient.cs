using NzbDrone.Api.Commands;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class CommandClient : ClientBase<CommandResource>
    {
        public CommandClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }
    }
}
