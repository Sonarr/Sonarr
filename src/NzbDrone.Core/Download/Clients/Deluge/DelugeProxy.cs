using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Rest;
using NLog;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public interface IDelugeProxy
    {
        String GetVersion(DelugeSettings settings);
        Dictionary<String, Object> GetConfig(DelugeSettings settings);
        DelugeTorrent[] GetTorrents(DelugeSettings settings);
        DelugeTorrent[] GetTorrentsByLabel(String label, DelugeSettings settings);
        String[] GetAvailablePlugins(DelugeSettings settings);
        String[] GetEnabledPlugins(DelugeSettings settings);
        String[] GetAvailableLabels(DelugeSettings settings);
        void SetLabel(String hash, String label, DelugeSettings settings);
        void SetTorrentConfiguration(String hash, String key, Object value, DelugeSettings settings);
        void SetTorrentSeedingConfiguration(String hash, TorrentSeedConfiguration seedConfiguration, DelugeSettings settings);
        void AddLabel(String label, DelugeSettings settings);
        String AddTorrentFromMagnet(String magnetLink, DelugeSettings settings);
        String AddTorrentFromFile(String filename, Byte[] fileContent, DelugeSettings settings);
        Boolean RemoveTorrent(String hash, Boolean removeData, DelugeSettings settings);
        void MoveTorrentToTopInQueue(String hash, DelugeSettings settings);
    }

    public class DelugeProxy : IDelugeProxy
    {
        private static readonly String[] requiredProperties = new String[] { "hash", "name", "state", "progress", "eta", "message", "is_finished", "save_path", "total_size", "total_done", "time_added", "active_time", "ratio", "is_auto_managed", "stop_at_ratio", "remove_at_ratio", "stop_ratio" };

        private readonly Logger _logger;

        private string _authPassword;
        private CookieContainer _authCookieContainer;

        private static Int32 _callId;

        public DelugeProxy(Logger logger)
        {
            _logger = logger;
        }

        public String GetVersion(DelugeSettings settings)
        {
            var response = ProcessRequest<String>(settings, "daemon.info");

            return response.Result;
        }

        public Dictionary<String, Object> GetConfig(DelugeSettings settings)
        {
            var response = ProcessRequest<Dictionary<String, Object>>(settings, "core.get_config");

            return response.Result;
        }

        public DelugeTorrent[] GetTorrents(DelugeSettings settings)
        {
            var filter = new Dictionary<String, Object>();

            // TODO: get_torrents_status returns the files as well, which starts to cause deluge timeouts when you get enough season packs.
            //var response = ProcessRequest<Dictionary<String, DelugeTorrent>>(settings, "core.get_torrents_status", filter, new String[0]);
            var response = ProcessRequest<DelugeUpdateUIResult>(settings, "web.update_ui", requiredProperties, filter);

            return response.Result.Torrents.Values.ToArray();
        }

        public DelugeTorrent[] GetTorrentsByLabel(String label, DelugeSettings settings)
        {
            var filter = new Dictionary<String, Object>();
            filter.Add("label", label);


            //var response = ProcessRequest<Dictionary<String, DelugeTorrent>>(settings, "core.get_torrents_status", filter, new String[0]);
            var response = ProcessRequest<DelugeUpdateUIResult>(settings, "web.update_ui", requiredProperties, filter);

            return response.Result.Torrents.Values.ToArray();
        }

        public String AddTorrentFromMagnet(String magnetLink, DelugeSettings settings)
        {
            var response = ProcessRequest<String>(settings, "core.add_torrent_magnet", magnetLink, new JObject());

            return response.Result;
        }

        public String AddTorrentFromFile(String filename, Byte[] fileContent, DelugeSettings settings)
        {
            var response = ProcessRequest<String>(settings, "core.add_torrent_file", filename, Convert.ToBase64String(fileContent), new JObject());

            return response.Result;
        }

        public Boolean RemoveTorrent(String hashString, Boolean removeData, DelugeSettings settings)
        {
            var response = ProcessRequest<Boolean>(settings, "core.remove_torrent", hashString, removeData);

            return response.Result;
        }

        public void MoveTorrentToTopInQueue(String hash, DelugeSettings settings)
        {
            ProcessRequest<Object>(settings, "core.queue_top", (Object)new String[] { hash });
        }

        public String[] GetAvailablePlugins(DelugeSettings settings)
        {
            var response = ProcessRequest<String[]>(settings, "core.get_available_plugins");

            return response.Result;
        }

        public String[] GetEnabledPlugins(DelugeSettings settings)
        {
            var response = ProcessRequest<String[]>(settings, "core.get_enabled_plugins");

            return response.Result;
        }

        public String[] GetAvailableLabels(DelugeSettings settings)
        {
            var response = ProcessRequest<String[]>(settings, "label.get_labels");

            return response.Result;
        }

        public void SetTorrentConfiguration(String hash, String key, Object value, DelugeSettings settings)
        {
            var arguments = new Dictionary<String, Object>();
            arguments.Add(key, value);

            ProcessRequest<Object>(settings, "core.set_torrent_options", new String[] { hash }, arguments);
        }

        public void SetTorrentSeedingConfiguration(String hash, TorrentSeedConfiguration seedConfiguration, DelugeSettings settings)
        {
            if (seedConfiguration.Ratio != null)
            {
                var ratioArguments = new Dictionary<String, Object>();
                ratioArguments.Add("stop_ratio", seedConfiguration.Ratio.Value);

                ProcessRequest<Object>(settings, "core.set_torrent_options", new String[]{hash}, ratioArguments);
            }
        }

        public void AddLabel(String label, DelugeSettings settings)
        {
            ProcessRequest<Object>(settings, "label.add", label);
        }

        public void SetLabel(String hash, String label, DelugeSettings settings)
        {
            ProcessRequest<Object>(settings, "label.set_torrent", hash, label);
        }

        protected DelugeResponse<TResult> ProcessRequest<TResult>(DelugeSettings settings, String action, params Object[] arguments)
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
        
        private DelugeResponse<TResult> ProcessRequest<TResult>(IRestClient client, String action, Object[] arguments)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "json";
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Accept-Encoding", "gzip,deflate");

            var data = new Dictionary<String, Object>();
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

            var url = String.Format(@"{0}://{1}:{2}",
                                 protocol,
                                 settings.Host,
                                 settings.Port);

            var restClient = RestClientFactory.BuildClient(url);
            restClient.Timeout = 4000;

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

            var result = ProcessRequest<Boolean>(restClient, "auth.login", new Object[] { _authPassword });

            if (!result.Result)
            {
                _logger.Debug("Deluge authentication failed.");
                throw new DownloadClientAuthenticationException("Failed to authenticate with Deluge.");
            }
            else
            {
                _logger.Debug("Deluge authentication succeeded.");
                _authCookieContainer = restClient.CookieContainer;
            }

            ConnectDaemon(restClient);
        }

        private void ConnectDaemon(IRestClient restClient)
        {
            var resultConnected = ProcessRequest<Boolean>(restClient, "web.connected", new Object[0]);

            if (resultConnected.Result)
            {
                return;
            }

            var resultHosts = ProcessRequest<List<Object[]>>(restClient, "web.get_hosts", new Object[0]);

            if (resultHosts.Result != null)
            {
                // The returned list contains the id, ip, port and status of each available connection. We want the 127.0.0.1
                var connection = resultHosts.Result.Where(v => "127.0.0.1" == (v[1] as String)).FirstOrDefault();

                if (connection != null)
                {
                    ProcessRequest<Object>(restClient, "web.connect", new Object[] { connection[0] });
                }
                else
                {
                    throw new DownloadClientException("Failed to connect to Deluge daemon.");
                }
            }
        }

        private Int32 GetCallId()
        {
            return System.Threading.Interlocked.Increment(ref _callId);
        }
    }
}
