using System;
using RestSharp;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.Rest
{
    public static class RestClientFactory
    {
        public static RestClient BuildClient(string baseUrl)
        {
            var restClient = new RestClient(baseUrl);

            restClient.UserAgent = string.Format("Sonarr/{0} (RestSharp/{1}; {2}/{3})",
                BuildInfo.Version,
                restClient.GetType().Assembly.GetName().Version,
                OsInfo.Os, OsInfo.Version.ToString(2));

            return restClient;
        }
    }
}
