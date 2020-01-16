using RestSharp;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http.Proxy;

namespace NzbDrone.Core.Rest
{
    public class RestClientFactory : IRestClientFactory
    {
        private readonly IHttpProxySettingsProvider _httpProxySettingsProvider;
        private readonly ICreateManagedWebProxy _createManagedWebProxy;

        public RestClientFactory(IHttpProxySettingsProvider httpProxySettingsProvider, ICreateManagedWebProxy createManagedWebProxy)
        {
            _httpProxySettingsProvider = httpProxySettingsProvider;
            _createManagedWebProxy = createManagedWebProxy;
        }

        public RestClient BuildClient(string baseUrl)
        {
            var restClient = new RestClient(baseUrl)
            {
                UserAgent = $"{BuildInfo.AppName}/{BuildInfo.Version} ({OsInfo.Os})"
            };

            var proxySettings = _httpProxySettingsProvider.GetProxySettings();
            if (proxySettings != null)
            {
                restClient.Proxy = _createManagedWebProxy.GetWebProxy(proxySettings);
            }

            return restClient;
        }
    }
}
