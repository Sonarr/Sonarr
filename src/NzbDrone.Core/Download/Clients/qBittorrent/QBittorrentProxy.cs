using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Rest;
using RestSharp;
using NzbDrone.Common.Cache;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    // API https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-Documentation

    public interface IQBittorrentProxy
    {
        int GetVersion(QBittorrentSettings settings);
        Dictionary<string, Object> GetConfig(QBittorrentSettings settings);
        List<QBittorrentTorrent> GetTorrents(QBittorrentSettings settings);

        void AddTorrentFromUrl(string torrentUrl, QBittorrentSettings settings);
        void AddTorrentFromFile(string fileName, Byte[] fileContent, QBittorrentSettings settings);

        void RemoveTorrent(string hash, Boolean removeData, QBittorrentSettings settings);
        void SetTorrentLabel(string hash, string label, QBittorrentSettings settings);
        void MoveTorrentToTopInQueue(string hash, QBittorrentSettings settings);
    }

    public class QBittorrentProxy : IQBittorrentProxy
    {
        private readonly Logger _logger;
        private readonly CookieContainer _cookieContainer;
        private readonly ICached<bool> _logins;
        private readonly TimeSpan _loginTimeout = TimeSpan.FromSeconds(10);

        public QBittorrentProxy(ICacheManager cacheManager, Logger logger)
        {
            _logger = logger;
            _cookieContainer = new CookieContainer();
            _logins = cacheManager.GetCache<bool>(GetType(), "logins");
        }

        public int GetVersion(QBittorrentSettings settings)
        {
            var request = new RestRequest("/version/api", Method.GET);

            var client = BuildClient(settings);
            var response = ProcessRequest(client, request, settings);
            response.ValidateResponse(client);
            return Convert.ToInt32(response.Content);
        }

        public Dictionary<string, Object> GetConfig(QBittorrentSettings settings)
        {
            var request = new RestRequest("/query/preferences", Method.GET);
            request.RequestFormat = DataFormat.Json;

            var client = BuildClient(settings);
            var response = ProcessRequest(client, request, settings);
            response.ValidateResponse(client);
            return response.Read<Dictionary<string, Object>>(client);
        }

        public List<QBittorrentTorrent> GetTorrents(QBittorrentSettings settings)
        {
            var request = new RestRequest("/query/torrents", Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("label", settings.TvCategory);

            var client = BuildClient(settings);
            var response = ProcessRequest(client, request, settings);
            response.ValidateResponse(client);
            return response.Read<List<QBittorrentTorrent>>(client);
        }

        public void AddTorrentFromUrl(string torrentUrl, QBittorrentSettings settings)
        {
            var request = new RestRequest("/command/download", Method.POST);
            request.AddParameter("urls", torrentUrl);

            var client = BuildClient(settings);
            var response = ProcessRequest(client, request, settings);
            response.ValidateResponse(client);
        }

        public void AddTorrentFromFile(string fileName, Byte[] fileContent, QBittorrentSettings settings)
        {
            var request = new RestRequest("/command/upload", Method.POST);
            request.AddFile("torrents", fileContent, fileName);

            var client = BuildClient(settings);
            var response = ProcessRequest(client, request, settings);
            response.ValidateResponse(client);
        }

        public void RemoveTorrent(string hash, Boolean removeData, QBittorrentSettings settings)
        {
            var cmd = removeData ? "/command/deletePerm" : "/command/delete";
            var request = new RestRequest(cmd, Method.POST);
            request.AddParameter("hashes", hash);

            var client = BuildClient(settings);
            var response = ProcessRequest(client, request, settings);
            response.ValidateResponse(client);
        }

        public void SetTorrentLabel(string hash, string label, QBittorrentSettings settings)
        {
            var request = new RestRequest("/command/setLabel", Method.POST);
            request.AddParameter("hashes", hash);
            request.AddParameter("label", label);

            var client = BuildClient(settings);
            var response = ProcessRequest(client, request, settings);
            response.ValidateResponse(client);
        }

        public void MoveTorrentToTopInQueue(string hash, QBittorrentSettings settings)
        {
            var request = new RestRequest("/command/topPrio", Method.POST);
            request.AddParameter("hashes", hash);

            var client = BuildClient(settings);
            var response = ProcessRequest(client, request, settings);

            // qBittorrent rejects all Prio commands with 403: Forbidden if Options -> BitTorrent -> Torrent Queueing is not enabled
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return;
            }

            response.ValidateResponse(client);
        }

        private IRestResponse ProcessRequest(IRestClient client, IRestRequest request, QBittorrentSettings settings)
        {
            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.Info("Authentication required, logging in.");

                var loggedIn = _logins.Get(settings.Username + settings.Password, () => Login(client, settings), _loginTimeout);

                if (!loggedIn)
                {
                    throw new DownloadClientAuthenticationException("Failed to authenticate");
                }

                // success! retry the original request
                response = client.Execute(request);
            }

            return response;
        }

        private bool Login(IRestClient client, QBittorrentSettings settings)
        {
            var request = new RestRequest("/login", Method.POST);
            request.AddParameter("username", settings.Username);
            request.AddParameter("password", settings.Password);

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Warn("Login failed with {0}.", response.StatusCode);
                return false;
            }

            if (response.Content != "Ok.") // returns "Fails." on bad login
            {
                _logger.Warn("Login failed, incorrect username or password.");
                return false;
            }

            response.ValidateResponse(client);
            return true;
        }

        private IRestClient BuildClient(QBittorrentSettings settings)
        {
            var protocol = settings.UseSsl ? "https" : "http";
            var url = String.Format(@"{0}://{1}:{2}", protocol, settings.Host, settings.Port);
            var client = RestClientFactory.BuildClient(url);

            client.Authenticator = new DigestAuthenticator(settings.Username, settings.Password);
            client.CookieContainer = _cookieContainer;
            return client;
        }
    }
}
