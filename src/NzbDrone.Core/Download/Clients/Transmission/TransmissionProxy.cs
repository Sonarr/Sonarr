using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Rest;
using NLog;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public interface ITransmissionProxy
    {
        List<TransmissionTorrent> GetTorrents(TransmissionSettings settings);
        void AddTorrentFromUrl(string torrentUrl, string downloadDirectory, TransmissionSettings settings);
        void AddTorrentFromData(byte[] torrentData, string downloadDirectory, TransmissionSettings settings);
        void SetTorrentSeedingConfiguration(string hash, TorrentSeedConfiguration seedConfiguration, TransmissionSettings settings);
        Dictionary<string, object> GetConfig(TransmissionSettings settings);
        string GetVersion(TransmissionSettings settings);
        void RemoveTorrent(string hash, bool removeData, TransmissionSettings settings);
        void MoveTorrentToTopInQueue(string hashString, TransmissionSettings settings);
    }

    public class TransmissionProxy: ITransmissionProxy
    {        
        private readonly Logger _logger;
        private string _sessionId;

        public TransmissionProxy(Logger logger)
        {
            _logger = logger;
        }
        
        public List<TransmissionTorrent> GetTorrents(TransmissionSettings settings)
        {
            var result = GetTorrentStatus(settings);

            var torrents = ((JArray)result.Arguments["torrents"]).ToObject<List<TransmissionTorrent>>();

            return torrents;
        }

        public void AddTorrentFromUrl(string torrentUrl, string downloadDirectory, TransmissionSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("filename", torrentUrl);

            if (!downloadDirectory.IsNullOrWhiteSpace())
            {
                arguments.Add("download-dir", downloadDirectory);
            }

            ProcessRequest("torrent-add", arguments, settings);
        }

        public void AddTorrentFromData(byte[] torrentData, string downloadDirectory, TransmissionSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("metainfo", Convert.ToBase64String(torrentData));

            if (!downloadDirectory.IsNullOrWhiteSpace())
            {
                arguments.Add("download-dir", downloadDirectory);
            }

            ProcessRequest("torrent-add", arguments, settings);
        }

        public void SetTorrentSeedingConfiguration(string hash, TorrentSeedConfiguration seedConfiguration, TransmissionSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("ids", new string[] { hash });

            if (seedConfiguration.Ratio != null)
            {
                arguments.Add("seedRatioLimit", seedConfiguration.Ratio.Value);
                arguments.Add("seedRatioMode", 1);
            }

            if (seedConfiguration.SeedTime != null)
            {
                arguments.Add("seedIdleLimit", Convert.ToInt32(seedConfiguration.SeedTime.Value.TotalMinutes));
                arguments.Add("seedIdleMode", 1);
            }

            ProcessRequest("torrent-set", arguments, settings);
        }

        public string GetVersion(TransmissionSettings settings)
        {
            // Gets the transmission version.
            var config = GetConfig(settings);

            var version = config["version"];

            return version.ToString();
        }

        public Dictionary<string, object> GetConfig(TransmissionSettings settings)
        {
            // Gets the transmission version.
            var result = GetSessionVariables(settings);

            return result.Arguments;
        }

        public void RemoveTorrent(string hashString, bool removeData, TransmissionSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("ids", new string[] { hashString });
            arguments.Add("delete-local-data", removeData);

            ProcessRequest("torrent-remove", arguments, settings);
        }

        public void MoveTorrentToTopInQueue(string hashString, TransmissionSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("ids", new string[] { hashString });

            ProcessRequest("queue-move-top", arguments, settings);
        }

        private TransmissionResponse GetSessionVariables(TransmissionSettings settings)
        {
            // Retrieve transmission information such as the default download directory, bandwith throttling and seed ratio.

            return ProcessRequest("session-get", null, settings);
        }

        private TransmissionResponse GetSessionStatistics(TransmissionSettings settings)
        {
            return ProcessRequest("session-stats", null, settings);
        }

        private TransmissionResponse GetTorrentStatus(TransmissionSettings settings)
        {
            return GetTorrentStatus(null, settings);
        }

        private TransmissionResponse GetTorrentStatus(IEnumerable<string> hashStrings, TransmissionSettings settings)
        {
            var fields = new string[]{
                "id",
                "hashString", // Unique torrent ID. Use this instead of the client id?
                "name",
                "downloadDir",
                "status",
                "totalSize",
                "leftUntilDone",
                "isFinished",
                "eta",
                "errorString"
            };
            
            var arguments = new Dictionary<string, object>();
            arguments.Add("fields", fields);

            if (hashStrings != null)
            {
                arguments.Add("ids", hashStrings);
            }

            var result = ProcessRequest("torrent-get", arguments, settings);

            return result;
        }

        protected string GetSessionId(IRestClient client, TransmissionSettings settings)
        {
            var request = new RestRequest();
            request.RequestFormat = DataFormat.Json;

            _logger.Debug("Url: {0} GetSessionId", client.BuildUri(request));
            var restResponse = client.Execute(request);

            if (restResponse.StatusCode == HttpStatusCode.MovedPermanently)
            {
                var uri = new Uri(restResponse.ResponseUri, (string)restResponse.GetHeaderValue("Location"));

                throw new DownloadClientException("Remote site redirected to " + uri);
            }

            // We expect the StatusCode = Conflict, coz that will provide us with a new session id.
            switch (restResponse.StatusCode)
            {
                case HttpStatusCode.Conflict:
                {
                    var sessionId = restResponse.Headers.SingleOrDefault(o => o.Name == "X-Transmission-Session-Id");

                    if (sessionId == null)
                    {
                        throw new DownloadClientException("Remote host did not return a Session Id.");
                    }

                    return (string)sessionId.Value;
                }
                case HttpStatusCode.Unauthorized:
                    throw new DownloadClientAuthenticationException("User authentication failed.");
            }

            restResponse.ValidateResponse(client);

            throw new DownloadClientException("Remote host did not return a Session Id.");
        }
        
        public TransmissionResponse ProcessRequest(string action, object arguments, TransmissionSettings settings)
        {
            var client = BuildClient(settings);

            if (string.IsNullOrWhiteSpace(_sessionId))
            {
                _sessionId = GetSessionId(client, settings);
            }

            var request = new RestRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-Transmission-Session-Id", _sessionId);

            var data = new Dictionary<string, object>();
            data.Add("method", action);

            if (arguments != null)
            {
                data.Add("arguments", arguments);
            }

            request.AddBody(data);

            _logger.Debug("Url: {0} Action: {1}", client.BuildUri(request), action);
            var restResponse = client.Execute(request);

            if (restResponse.StatusCode == HttpStatusCode.Conflict)
            {
                _sessionId = GetSessionId(client, settings);
                request.Parameters.First(o => o.Name == "X-Transmission-Session-Id").Value = _sessionId;
                restResponse = client.Execute(request);
            }
            else if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new DownloadClientAuthenticationException("User authentication failed.");
            }

            var transmissionResponse = restResponse.Read<TransmissionResponse>(client);

            if (transmissionResponse == null)
            {
                throw new TransmissionException("Unexpected response");
            }
            else if (transmissionResponse.Result != "success")
            {
                throw new TransmissionException(transmissionResponse.Result);
            }

            return transmissionResponse;
        }

        private IRestClient BuildClient(TransmissionSettings settings)
        {
            var protocol = settings.UseSsl ? "https" : "http";
            
            var url = string.Format(@"{0}://{1}:{2}/{3}/rpc", protocol, settings.Host, settings.Port, settings.UrlBase.Trim('/'));

            var restClient = RestClientFactory.BuildClient(url);
            restClient.FollowRedirects = false;

            if (!settings.Username.IsNullOrWhiteSpace())
            {
                restClient.Authenticator = new HttpBasicAuthenticator(settings.Username, settings.Password);
            }

            return restClient;
        }
    }
}
