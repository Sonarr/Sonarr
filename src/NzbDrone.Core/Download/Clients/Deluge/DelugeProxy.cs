using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public interface IDelugeProxy
    {
        string GetVersion(DelugeSettings settings);
        Dictionary<string, object> GetConfig(DelugeSettings settings);
        DelugeTorrent[] GetTorrents(DelugeSettings settings);
        DelugeTorrent[] GetTorrentsByLabel(string label, DelugeSettings settings);
        string[] GetAvailablePlugins(DelugeSettings settings);
        string[] GetEnabledPlugins(DelugeSettings settings);
        string[] GetAvailableLabels(DelugeSettings settings);
        void SetTorrentLabel(string hash, string label, DelugeSettings settings);
        void SetTorrentConfiguration(string hash, string key, object value, DelugeSettings settings);
        void SetTorrentSeedingConfiguration(string hash, TorrentSeedConfiguration seedConfiguration, DelugeSettings settings);
        void AddLabel(string label, DelugeSettings settings);
        string AddTorrentFromMagnet(string magnetLink, DelugeSettings settings);
        string AddTorrentFromFile(string filename, byte[] fileContent, DelugeSettings settings);
        bool RemoveTorrent(string hash, bool removeData, DelugeSettings settings);
        void MoveTorrentToTopInQueue(string hash, DelugeSettings settings);
    }

    public class DelugeProxy : IDelugeProxy
    {
        private static readonly string[] RequiredProperties = new string[] { "hash", "name", "state", "progress", "eta", "message", "is_finished", "save_path", "total_size", "total_done", "time_added", "active_time", "ratio", "is_auto_managed", "stop_at_ratio", "remove_at_ratio", "stop_ratio" };

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private readonly ICached<Dictionary<string, string>> _authCookieCache;

        public DelugeProxy(ICacheManager cacheManager, IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _authCookieCache = cacheManager.GetCache<Dictionary<string, string>>(GetType(), "authCookies");
        }

        public string GetVersion(DelugeSettings settings)
        {
            try
            {
                var response = ProcessRequest<string>(settings, "daemon.info");

                return response;
            }
            catch (DownloadClientException ex)
            {
                if (ex.Message.Contains("Unknown method"))
                {
                    // Deluge v2 beta replaced 'daemon.info' with 'daemon.get_version'.
                    // It may return or become official, for now we just retry with the get_version api.
                    var response = ProcessRequest<string>(settings, "daemon.get_version");

                    return response;
                }

                throw;
            }
        }

        public Dictionary<string, object> GetConfig(DelugeSettings settings)
        {
            var response = ProcessRequest<Dictionary<string, object>>(settings, "core.get_config");

            return response;
        }

        public DelugeTorrent[] GetTorrents(DelugeSettings settings)
        {
            var filter = new Dictionary<string, object>();

            // TODO: get_torrents_status returns the files as well, which starts to cause deluge timeouts when you get enough season packs.
            //var response = ProcessRequest<Dictionary<String, DelugeTorrent>>(settings, "core.get_torrents_status", filter, new String[0]);
            var response = ProcessRequest<DelugeUpdateUIResult>(settings, "web.update_ui", RequiredProperties, filter);

            return GetTorrents(response);
        }

        public DelugeTorrent[] GetTorrentsByLabel(string label, DelugeSettings settings)
        {
            var filter = new Dictionary<string, object>();
            filter.Add("label", label);

            //var response = ProcessRequest<Dictionary<String, DelugeTorrent>>(settings, "core.get_torrents_status", filter, new String[0]);
            var response = ProcessRequest<DelugeUpdateUIResult>(settings, "web.update_ui", RequiredProperties, filter);

            return GetTorrents(response);
        }

        public string AddTorrentFromMagnet(string magnetLink, DelugeSettings settings)
        {
            var options = new
                          {
                              add_paused = settings.AddPaused,
                              remove_at_ratio = false
                          };

            var response = ProcessRequest<string>(settings, "core.add_torrent_magnet", magnetLink, options);

            return response;
        }

        public string AddTorrentFromFile(string filename, byte[] fileContent, DelugeSettings settings)
        {
            var options = new
                          {
                              add_paused = settings.AddPaused,
                              remove_at_ratio = false
                          };

            var response = ProcessRequest<string>(settings, "core.add_torrent_file", filename, fileContent, options);

            return response;
        }

        public bool RemoveTorrent(string hash, bool removeData, DelugeSettings settings)
        {
            var response = ProcessRequest<bool>(settings, "core.remove_torrent", hash, removeData);

            return response;
        }

        public void MoveTorrentToTopInQueue(string hash, DelugeSettings settings)
        {
            ProcessRequest<object>(settings, "core.queue_top", (object)new string[] { hash });
        }

        public string[] GetAvailablePlugins(DelugeSettings settings)
        {
            var response = ProcessRequest<string[]>(settings, "core.get_available_plugins");

            return response;
        }

        public string[] GetEnabledPlugins(DelugeSettings settings)
        {
            var response = ProcessRequest<string[]>(settings, "core.get_enabled_plugins");

            return response;
        }

        public string[] GetAvailableLabels(DelugeSettings settings)
        {
            var response = ProcessRequest<string[]>(settings, "label.get_labels");

            return response;
        }

        public void SetTorrentConfiguration(string hash, string key, object value, DelugeSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add(key, value);

            ProcessRequest<object>(settings, "core.set_torrent_options", new string[] { hash }, arguments);
        }

        public void SetTorrentSeedingConfiguration(string hash, TorrentSeedConfiguration seedConfiguration, DelugeSettings settings)
        {
            if (seedConfiguration == null)
            {
                return;
            }

            var ratioArguments = new Dictionary<string, object>();

            if (seedConfiguration.Ratio != null)
            {
                ratioArguments.Add("stop_ratio", seedConfiguration.Ratio.Value);
                ratioArguments.Add("stop_at_ratio", 1);
            }

            ProcessRequest<object>(settings, "core.set_torrent_options", new[] { hash }, ratioArguments);
        }

        public void AddLabel(string label, DelugeSettings settings)
        {
            ProcessRequest<object>(settings, "label.add", label);
        }

        public void SetTorrentLabel(string hash, string label, DelugeSettings settings)
        {
            ProcessRequest<object>(settings, "label.set_torrent", hash, label);
        }

        private JsonRpcRequestBuilder BuildRequest(DelugeSettings settings)
        {
            string url = HttpRequestBuilder.BuildBaseUrl(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase);

            var requestBuilder = new JsonRpcRequestBuilder(url);
            requestBuilder.LogResponseContent = true;

            requestBuilder.Resource("json");
            requestBuilder.PostProcess += r => r.RequestTimeout = TimeSpan.FromSeconds(15);

            AuthenticateClient(requestBuilder, settings);

            return requestBuilder;
        }

        protected TResult ProcessRequest<TResult>(DelugeSettings settings, string method, params object[] arguments)
        {
            var requestBuilder = BuildRequest(settings);

            var response = ExecuteRequest<TResult>(requestBuilder, method, arguments);

            if (response.Error != null)
            {
                var error = response.Error.ToObject<DelugeError>();
                if (error.Code == 1 || error.Code == 2)
                {
                    AuthenticateClient(requestBuilder, settings, true);

                    response = ExecuteRequest<TResult>(requestBuilder, method, arguments);

                    if (response.Error == null)
                    {
                        return response.Result;
                    }

                    error = response.Error.ToObject<DelugeError>();

                    throw new DownloadClientAuthenticationException(error.Message);
                }

                throw new DelugeException(error.Message, error.Code);
            }

            return response.Result;
        }

        private JsonRpcResponse<TResult> ExecuteRequest<TResult>(JsonRpcRequestBuilder requestBuilder, string method, params object[] arguments)
        {
            var request = requestBuilder.Call(method, arguments).Build();

            HttpResponse response;
            try
            {
                response = _httpClient.Execute(request);

                return Json.Deserialize<JsonRpcResponse<TResult>>(response.Content);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.RequestTimeout)
                {
                    _logger.Debug("Deluge timeout during request, daemon connection may have been broken. Attempting to reconnect.");
                    return new JsonRpcResponse<TResult>()
                    {
                        Error = JToken.Parse("{ Code = 2 }")
                    };
                }
                else
                {
                    throw new DownloadClientException("Unable to connect to Deluge, please check your settings", ex);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.TrustFailure)
                {
                    throw new DownloadClientUnavailableException("Unable to connect to Deluge, certificate validation failed.", ex);
                }

                throw new DownloadClientUnavailableException("Unable to connect to Deluge, please check your settings", ex);
            }
        }

        private void VerifyResponse<TResult>(JsonRpcResponse<TResult> response)
        {
            if (response.Error != null)
            {
                var error = response.Error.ToObject<DelugeError>();
                throw new DelugeException(error.Message, error.Code);
            }
        }

        private void AuthenticateClient(JsonRpcRequestBuilder requestBuilder, DelugeSettings settings, bool reauthenticate = false)
        {
            var authKey = string.Format("{0}:{1}", requestBuilder.BaseUrl, settings.Password);

            var cookies = _authCookieCache.Find(authKey);

            if (cookies == null || reauthenticate)
            {
                _authCookieCache.Remove(authKey);

                var authLoginRequest = requestBuilder.Call("auth.login", settings.Password).Build();
                var response = _httpClient.Execute(authLoginRequest);
                var result = Json.Deserialize<JsonRpcResponse<bool>>(response.Content);
                if (!result.Result)
                {
                    _logger.Debug("Deluge authentication failed.");
                    throw new DownloadClientAuthenticationException("Failed to authenticate with Deluge.");
                }

                _logger.Debug("Deluge authentication succeeded.");

                cookies = response.GetCookies();

                _authCookieCache.Set(authKey, cookies);

                requestBuilder.SetCookies(cookies);

                ConnectDaemon(requestBuilder);
            }
            else
            {
                requestBuilder.SetCookies(cookies);
            }
        }

        private void ConnectDaemon(JsonRpcRequestBuilder requestBuilder)
        {
            var resultConnected = ExecuteRequest<bool>(requestBuilder, "web.connected");
            VerifyResponse(resultConnected);

            if (resultConnected.Result)
            {
                return;
            }

            var resultHosts = ExecuteRequest<List<object[]>>(requestBuilder, "web.get_hosts");
            VerifyResponse(resultHosts);

            if (resultHosts.Result != null)
            {
                // The returned list contains the id, ip, port and status of each available connection. We want the 127.0.0.1
                var connection = resultHosts.Result.FirstOrDefault(v => (v[1] as string) == "127.0.0.1");

                if (connection != null)
                {
                    var resultConnect = ExecuteRequest<object>(requestBuilder, "web.connect", new object[] { connection[0] });
                    VerifyResponse(resultConnect);

                    return;
                }
            }

            throw new DownloadClientException("Failed to connect to Deluge daemon.");
        }

        private DelugeTorrent[] GetTorrents(DelugeUpdateUIResult result)
        {
            if (result.Torrents == null)
            {
                return new DelugeTorrent[0];
            }

            return result.Torrents.Values.ToArray();
        }
    }
}
