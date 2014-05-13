using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Common;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Rest;
using NLog;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public interface ITransmissionProxy
    {
        List<TransmissionTorrent> GetTorrents(TransmissionSettings settings);
        void AddTorrentFromUrl(String torrentUrl, String downloadDirectory, TransmissionSettings settings);
        void AddTorrentFromData(Byte[] torrentData, String downloadDirectory, TransmissionSettings settings);
        void SetTorrentSeedingConfiguration(String hash, TorrentSeedConfiguration seedConfiguration, TransmissionSettings settings);
        Dictionary<String, Object> GetConfig(TransmissionSettings settings);
        String GetVersion(TransmissionSettings settings);
        void RemoveTorrent(String hash, Boolean removeData, TransmissionSettings settings);
        void MoveTorrentToTopInQueue(String hashString, TransmissionSettings settings);
    }

    public class TransmissionProxy: ITransmissionProxy
    {        
        private readonly Logger _logger;
        private String _sessionId;

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

        public void AddTorrentFromUrl(String torrentUrl, String downloadDirectory, TransmissionSettings settings)
        {
            var arguments = new Dictionary<String, Object>();
            arguments.Add("filename", torrentUrl);

            if (!downloadDirectory.IsNullOrWhiteSpace())
            {
                arguments.Add("download-dir", downloadDirectory);
            }

            ProcessRequest("torrent-add", arguments, settings);
        }

        public void AddTorrentFromData(Byte[] torrentData, String downloadDirectory, TransmissionSettings settings)
        {
            var arguments = new Dictionary<String, Object>();
            arguments.Add("metainfo", Convert.ToBase64String(torrentData));

            if (!downloadDirectory.IsNullOrWhiteSpace())
            {
                arguments.Add("download-dir", downloadDirectory);
            }

            ProcessRequest("torrent-add", arguments, settings);
        }

        public void SetTorrentSeedingConfiguration(String hash, TorrentSeedConfiguration seedConfiguration, TransmissionSettings settings)
        {
            var arguments = new Dictionary<String, Object>();
            arguments.Add("ids", new String[] { hash });

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

        public String GetVersion(TransmissionSettings settings)
        {
            // Gets the transmission version.
            var config = GetConfig(settings);

            var version = config["version"];

            return version.ToString();
        }

        public Dictionary<String, Object> GetConfig(TransmissionSettings settings)
        {
            // Gets the transmission version.
            var result = GetSessionVariables(settings);

            return result.Arguments;
        }

        public void RemoveTorrent(String hashString, Boolean removeData, TransmissionSettings settings)
        {
            var arguments = new Dictionary<String, Object>();
            arguments.Add("ids", new String[] { hashString });
            arguments.Add("delete-local-data", removeData);

            ProcessRequest("torrent-remove", arguments, settings);
        }

        public void MoveTorrentToTopInQueue(String hashString, TransmissionSettings settings)
        {
            var arguments = new Dictionary<String, Object>();
            arguments.Add("ids", new String[] { hashString });

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

        private TransmissionResponse GetTorrentStatus(IEnumerable<String> hashStrings, TransmissionSettings settings)
        {
            var fields = new String[]{
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
            
            var arguments = new Dictionary<String, Object>();
            arguments.Add("fields", fields);

            if (hashStrings != null)
            {
                arguments.Add("ids", hashStrings);
            }

            var result = ProcessRequest("torrent-get", arguments, settings);

            return result;
        }

        protected String GetSessionId(IRestClient client, TransmissionSettings settings)
        {
            var request = new RestRequest();
            request.RequestFormat = DataFormat.Json;

            _logger.Debug("Url: {0} GetSessionId", client.BuildUri(request));
            var restResponse = client.Execute(request);

            if (restResponse.StatusCode == HttpStatusCode.MovedPermanently)
            {
                var uri = new Uri(restResponse.ResponseUri, (String)restResponse.GetHeaderValue("Location"));

                throw new DownloadClientException("Remote site redirected to " + uri);
            }

            // We expect the StatusCode = Conflict, coz that will provide us with a new session id.
            if (restResponse.StatusCode == HttpStatusCode.Conflict)
            {
                var sessionId = restResponse.Headers.SingleOrDefault(o => o.Name == "X-Transmission-Session-Id");

                if (sessionId == null)
                {
                    throw new DownloadClientException("Remote host did not return a Session Id.");
                }

                return (String)sessionId.Value;
            }
            else if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new DownloadClientAuthenticationException("User authentication failed.");
            }

            restResponse.ValidateResponse(client);

            throw new DownloadClientException("Remote host did not return a Session Id.");
        }
        
        public TransmissionResponse ProcessRequest(String action, Object arguments, TransmissionSettings settings)
        {
            var client = BuildClient(settings);

            if (String.IsNullOrWhiteSpace(_sessionId))
            {
                _sessionId = GetSessionId(client, settings);
            }

            var request = new RestRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-Transmission-Session-Id", _sessionId);

            var data = new Dictionary<String, Object>();
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
            
            String url;
            if (!settings.UrlBase.IsNullOrWhiteSpace())
            {
                url = String.Format(@"{0}://{1}:{2}/{3}/transmission/rpc", protocol, settings.Host, settings.Port, settings.UrlBase.Trim('/'));
            }
            else
            {
                url = String.Format(@"{0}://{1}:{2}/transmission/rpc", protocol, settings.Host, settings.Port);
            }

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
