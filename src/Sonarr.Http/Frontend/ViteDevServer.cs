using System;
using System.Net.Http;
using System.Threading.Tasks;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace Sonarr.Http.Frontend
{
    public interface IViteDevServer
    {
        bool IsEnabled { get; }
        bool HandlesPath(string resourceUrl);
        Task<HttpResponseMessage> GetAsync(string resourceUrl, string queryString);
        Task<string> GetIndexHtmlAsync();
    }

    public class ViteDevServer : IViteDevServer
    {
        private static readonly string[] Prefixes =
        {
            "/@vite/", "/@react-refresh", "/@id/", "/@fs/", "/node_modules/", "/frontend/src/"
        };

        private readonly HttpClient _httpClient;
        private readonly string _baseAddress;

        public ViteDevServer(string baseAddress = null)
        {
            _baseAddress = baseAddress ?? Environment.GetEnvironmentVariable("SONARR_VITE_DEV_SERVER");
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        protected virtual bool IsDebugBuild => BuildInfo.IsDebug;

        public bool IsEnabled => IsDebugBuild && _baseAddress.IsNotNullOrWhiteSpace();

        public static bool IsViteDevPath(string resourceUrl)
        {
            var path = resourceUrl.ToLowerInvariant();

            foreach (var prefix in Prefixes)
            {
                if (path.StartsWith(prefix) || path.Equals(prefix.TrimEnd('/')))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HandlesPath(string resourceUrl)
        {
            return IsEnabled && IsViteDevPath(resourceUrl);
        }

        public Task<HttpResponseMessage> GetAsync(string resourceUrl, string queryString)
        {
            return _httpClient.GetAsync($"{_baseAddress}{resourceUrl}{queryString}");
        }

        public Task<string> GetIndexHtmlAsync()
        {
            return _httpClient.GetStringAsync($"{_baseAddress}/index.html");
        }
    }
}
