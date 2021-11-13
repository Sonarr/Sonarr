using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.Flood.Types;

namespace NzbDrone.Core.Download.Clients.Flood
{
    public interface IFloodProxy
    {
        void AuthVerify(FloodSettings settings);
        void AddTorrentByUrl(string url, IEnumerable<string> tags, FloodSettings settings);
        void AddTorrentByFile(string file, IEnumerable<string> tags, FloodSettings settings);
        void DeleteTorrent(string hash, bool deleteData, FloodSettings settings);
        Dictionary<string, Torrent> GetTorrents(FloodSettings settings);
        List<string> GetTorrentContentPaths(string hash, FloodSettings settings);
        void SetTorrentsTags(string hash, IEnumerable<string> tags, FloodSettings settings);
        FloodClientSettings GetClientSettings(FloodSettings settings);
    }

    public class FloodProxy : IFloodProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private readonly ICached<Dictionary<string, string>> _authCookieCache;

        public FloodProxy(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _authCookieCache = cacheManager.GetCache<Dictionary<string, string>>(GetType(), "authCookies");
        }

        private string BuildUrl(FloodSettings settings)
        {
            return $"{(settings.UseSsl ? "https://" : "http://")}{settings.Host}:{settings.Port}/{settings.UrlBase}";
        }

        private string BuildCachedCookieKey(FloodSettings settings)
        {
            return $"{BuildUrl(settings)}:{settings.Username}";
        }

        private HttpRequestBuilder BuildRequest(FloodSettings settings)
        {
            var requestBuilder = new HttpRequestBuilder(HttpUri.CombinePath(BuildUrl(settings), "/api"))
            {
                LogResponseContent = true,
                NetworkCredential = new NetworkCredential(settings.Username, settings.Password)
            };

            requestBuilder.Headers.ContentType = "application/json";
            requestBuilder.SetCookies(AuthAuthenticate(requestBuilder, settings));

            return requestBuilder;
        }

        private HttpResponse HandleRequest(HttpRequest request, FloodSettings settings)
        {
            try
            {
                return _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Forbidden ||
                    ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authCookieCache.Remove(BuildCachedCookieKey(settings));
                    throw new DownloadClientAuthenticationException("Failed to authenticate with Flood.");
                }

                throw new DownloadClientException("Unable to connect to Flood, please check your settings");
            }
            catch
            {
                throw new DownloadClientException("Unable to connect to Flood, please check your settings");
            }
        }

        private Dictionary<string, string> AuthAuthenticate(HttpRequestBuilder requestBuilder, FloodSettings settings, bool force = false)
        {
            var cachedCookies = _authCookieCache.Find(BuildCachedCookieKey(settings));

            if (cachedCookies == null || force)
            {
                var authenticateRequest = requestBuilder.Resource("/auth/authenticate").Post().Build();

                var body = new Dictionary<string, object>
                {
                    { "username", settings.Username },
                    { "password", settings.Password }
                };
                authenticateRequest.SetContent(body.ToJson());

                var response = HandleRequest(authenticateRequest, settings);
                cachedCookies = response.GetCookies();
                _authCookieCache.Set(BuildCachedCookieKey(settings), cachedCookies);
            }

            return cachedCookies;
        }

        public void AuthVerify(FloodSettings settings)
        {
            var verifyRequest = BuildRequest(settings).Resource("/auth/verify").Build();

            verifyRequest.Method = HttpMethod.Get;

            HandleRequest(verifyRequest, settings);
        }

        public void AddTorrentByFile(string file, IEnumerable<string> tags, FloodSettings settings)
        {
            var addRequest = BuildRequest(settings).Resource("/torrents/add-files").Post().Build();

            var body = new Dictionary<string, object>
            {
                { "files", new List<string> { file } },
                { "tags", tags.ToList() }
            };

            if (settings.Destination != null)
            {
                body.Add("destination", settings.Destination);
            }

            if (settings.StartOnAdd)
            {
                body.Add("start", true);
            }

            addRequest.SetContent(body.ToJson());

            HandleRequest(addRequest, settings);
        }

        public void AddTorrentByUrl(string url, IEnumerable<string> tags, FloodSettings settings)
        {
            var addRequest = BuildRequest(settings).Resource("/torrents/add-urls").Post().Build();

            var body = new Dictionary<string, object>
            {
                { "urls", new List<string> { url } },
                { "tags", tags.ToList() }
            };

            if (settings.Destination != null)
            {
                body.Add("destination", settings.Destination);
            }

            if (settings.StartOnAdd)
            {
                body.Add("start", true);
            }

            addRequest.SetContent(body.ToJson());

            HandleRequest(addRequest, settings);
        }

        public void DeleteTorrent(string hash, bool deleteData, FloodSettings settings)
        {
            var deleteRequest = BuildRequest(settings).Resource("/torrents/delete").Post().Build();

            var body = new Dictionary<string, object>
            {
                { "hashes", new List<string> { hash } },
                { "deleteData", deleteData }
            };
            deleteRequest.SetContent(body.ToJson());

            HandleRequest(deleteRequest, settings);
        }

        public Dictionary<string, Torrent> GetTorrents(FloodSettings settings)
        {
            var getTorrentsRequest = BuildRequest(settings).Resource("/torrents").Build();

            getTorrentsRequest.Method = HttpMethod.Get;

            return Json.Deserialize<TorrentListSummary>(HandleRequest(getTorrentsRequest, settings).Content).Torrents;
        }

        public List<string> GetTorrentContentPaths(string hash, FloodSettings settings)
        {
            var contentsRequest = BuildRequest(settings).Resource($"/torrents/{hash}/contents").Build();

            contentsRequest.Method = HttpMethod.Get;

            return Json.Deserialize<List<TorrentContent>>(HandleRequest(contentsRequest, settings).Content).ConvertAll(content => content.Path);
        }

        public void SetTorrentsTags(string hash, IEnumerable<string> tags, FloodSettings settings)
        {
            var tagsRequest = BuildRequest(settings).Resource("/torrents/tags").Build();

            tagsRequest.Method = HttpMethod.Patch;

            var body = new Dictionary<string, object>
            {
                { "hashes", new List<string> { hash } },
                { "tags", tags.ToList() }
            };
            tagsRequest.SetContent(body.ToJson());

            HandleRequest(tagsRequest, settings);
        }

        public FloodClientSettings GetClientSettings(FloodSettings settings)
        {
            var contentsRequest = BuildRequest(settings).Resource($"/client/settings").Build();

            contentsRequest.Method = HttpMethod.Get;

            return Json.Deserialize<FloodClientSettings>(HandleRequest(contentsRequest, settings).Content);
        }
    }
}
