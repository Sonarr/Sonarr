using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Rest;
using RestSharp;

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
        void SetLabel(string hash, string label, DelugeSettings settings);
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
        private static readonly string[] requiredProperties = new string[] { "hash", "name", "state", "progress", "eta", "message", "is_finished", "save_path", "total_size", "total_done", "time_added", "active_time", "ratio", "is_auto_managed", "stop_at_ratio", "remove_at_ratio", "stop_ratio" };

        private readonly Logger _logger;

        private string _authPassword;
        private CookieContainer _authCookieContainer;

        private static int _callId;

        public DelugeProxy(Logger logger)
        {
            _logger = logger;
        }

        public string GetVersion(DelugeSettings settings)
        {
            var response = ProcessRequest<string>(settings, "daemon.info");

            return response.Result;
        }

        public Dictionary<string, object> GetConfig(DelugeSettings settings)
        {
            var response = ProcessRequest<Dictionary<string, object>>(settings, "core.get_config");

            return response.Result;
        }

        public DelugeTorrent[] GetTorrents(DelugeSettings settings)
        {
            var filter = new Dictionary<string, object>();

            // TODO: get_torrents_status returns the files as well, which starts to cause deluge timeouts when you get enough season packs.
            //var response = ProcessRequest<Dictionary<String, DelugeTorrent>>(settings, "core.get_torrents_status", filter, new String[0]);
            var response = ProcessRequest<DelugeUpdateUIResult>(settings, "web.update_ui", requiredProperties, filter);

            return GetTorrents(response.Result);
        }

        public DelugeTorrent[] GetTorrentsByLabel(string label, DelugeSettings settings)
        {
            var filter = new Dictionary<string, object>();
            filter.Add("label", label);

            //var response = ProcessRequest<Dictionary<String, DelugeTorrent>>(settings, "core.get_torrents_status", filter, new String[0]);
            var response = ProcessRequest<DelugeUpdateUIResult>(settings, "web.update_ui", requiredProperties, filter);

            return GetTorrents(response.Result);
        }

        public string AddTorrentFromMagnet(string magnetLink, DelugeSettings settings)
        {
            var response = ProcessRequest<string>(settings, "core.add_torrent_magnet", magnetLink, new JObject());

            return response.Result;
        }

        public string AddTorrentFromFile(string filename, byte[] fileContent, DelugeSettings settings)
        {
            var response = ProcessRequest<string>(settings, "core.add_torrent_file", filename, Convert.ToBase64String(fileContent), new JObject());

            return response.Result;
        }

        public bool RemoveTorrent(string hashString, bool removeData, DelugeSettings settings)
        {
            var response = ProcessRequest<bool>(settings, "core.remove_torrent", hashString, removeData);

            return response.Result;
        }

        public void MoveTorrentToTopInQueue(string hash, DelugeSettings settings)
        {
            ProcessRequest<object>(settings, "core.queue_top", (object)new string[] { hash });
        }

        public string[] GetAvailablePlugins(DelugeSettings settings)
        {
            var response = ProcessRequest<string[]>(settings, "core.get_available_plugins");

            return response.Result;
        }

        public string[] GetEnabledPlugins(DelugeSettings settings)
        {
            var response = ProcessRequest<string[]>(settings, "core.get_enabled_plugins");

            return response.Result;
        }

        public string[] GetAvailableLabels(DelugeSettings settings)
        {
            var response = ProcessRequest<string[]>(settings, "label.get_labels");

            return response.Result;
        }

        public void SetTorrentConfiguration(string hash, string key, object value, DelugeSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add(key, value);

            ProcessRequest<object>(settings, "core.set_torrent_options", new string[] { hash }, arguments);
        }

        public void SetTorrentSeedingConfiguration(string hash, TorrentSeedConfiguration seedConfiguration, DelugeSettings settings)
        {
            if (seedConfiguration.Ratio != null)
            {
                var ratioArguments = new Dictionary<string, object>();
                ratioArguments.Add("stop_ratio", seedConfiguration.Ratio.Value);

                ProcessRequest<object>(settings, "core.set_torrent_options", new string[]{hash}, ratioArguments);
            }
        }

        public void AddLabel(string label, DelugeSettings settings)
        {
            ProcessRequest<object>(settings, "label.add", label);
        }

        public void SetLabel(string hash, string label, DelugeSettings settings)
        {
            ProcessRequest<object>(settings, "label.set_torrent", hash, label);
        }

        protected DelugeResponse<TResult> ProcessRequest<TResult>(DelugeSettings settings, string action, params object[] arguments)
        {
            var client = BuildClient(settings);

            DelugeResponse<TResult> response;

            try
            {
                response = ProcessRequest<TResult>(client, action, arguments);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    _logger.Debug("Deluge timeout during request, daemon connection may have been broken. Attempting to reconnect.");
                    response = new DelugeResponse<TResult>();
                    response.Error = new DelugeError();
                    response.Error.Code = 2;
                }
                else
                {
                    throw;
                }
            }

            if (response.Error != null)
            {
                if (response.Error.Code == 1 || response.Error.Code == 2)
                {
                    AuthenticateClient(client);

                    response = ProcessRequest<TResult>(client, action, arguments);

                    if (response.Error == null)
                    {
                        return response;
                    }

                    throw new DownloadClientAuthenticationException(response.Error.Message);
                }

                throw new DelugeException(response.Error.Message, response.Error.Code);
            }

            return response;
        }
        
        private DelugeResponse<TResult> ProcessRequest<TResult>(IRestClient client, string action, object[] arguments)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "json";
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Accept-Encoding", "gzip,deflate");

            var data = new Dictionary<string, object>();
            data.Add("id", GetCallId());
            data.Add("method", action);

            if (arguments != null)
            {
                data.Add("params", arguments);
            }

            request.AddBody(data);

            _logger.Debug("Url: {0} Action: {1}", client.BuildUri(request), action);
            var response = client.ExecuteAndValidate<DelugeResponse<TResult>>(request);

            return response;
        }

        private IRestClient BuildClient(DelugeSettings settings)
        {
            var protocol = settings.UseSsl ? "https" : "http";

            string url;
            if (!settings.UrlBase.IsNullOrWhiteSpace())
            {
                url = string.Format(@"{0}://{1}:{2}/{3}", protocol, settings.Host, settings.Port, settings.UrlBase.Trim('/'));
            }
            else
            {
                url = string.Format(@"{0}://{1}:{2}", protocol, settings.Host, settings.Port);
            }

            var restClient = RestClientFactory.BuildClient(url);
            restClient.Timeout = 15000;

            if (_authPassword != settings.Password || _authCookieContainer == null)
            {
                _authPassword = settings.Password;
                AuthenticateClient(restClient);
            }
            else
            {
                restClient.CookieContainer = _authCookieContainer;
            }

            return restClient;
        }

        private void AuthenticateClient(IRestClient restClient)
        {
            restClient.CookieContainer = new CookieContainer();

            var result = ProcessRequest<bool>(restClient, "auth.login", new object[] { _authPassword });

            if (!result.Result)
            {
                _logger.Debug("Deluge authentication failed.");
                throw new DownloadClientAuthenticationException("Failed to authenticate with Deluge.");
            }
            _logger.Debug("Deluge authentication succeeded.");
            _authCookieContainer = restClient.CookieContainer;

            ConnectDaemon(restClient);
        }

        private void ConnectDaemon(IRestClient restClient)
        {
            var resultConnected = ProcessRequest<bool>(restClient, "web.connected", new object[0]);

            if (resultConnected.Result)
            {
                return;
            }

            var resultHosts = ProcessRequest<List<object[]>>(restClient, "web.get_hosts", new object[0]);

            if (resultHosts.Result != null)
            {
                // The returned list contains the id, ip, port and status of each available connection. We want the 127.0.0.1
                var connection = resultHosts.Result.FirstOrDefault(v => "127.0.0.1" == (v[1] as string));

                if (connection != null)
                {
                    ProcessRequest<object>(restClient, "web.connect", new object[] { connection[0] });
                }
                else
                {
                    throw new DownloadClientException("Failed to connect to Deluge daemon.");
                }
            }
        }

        private int GetCallId()
        {
            return System.Threading.Interlocked.Increment(ref _callId);
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
