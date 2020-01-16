using RestSharp;

namespace NzbDrone.Core.Rest
{
    public interface IRestClientFactory
    {
        RestClient BuildClient(string baseUrl);
    }
}
