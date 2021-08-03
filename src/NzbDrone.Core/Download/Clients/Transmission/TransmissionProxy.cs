using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public interface ITransmissionProxy
    {
        List<TransmissionTorrent> GetTorrents(TransmissionSettings settings);
        void AddTorrentFromUrl(string torrentUrl, string downloadDirectory, TransmissionSettings settings);
        void AddTorrentFromData(byte[] torrentData, string downloadDirectory, TransmissionSettings settings);
        void SetTorrentSeedingConfiguration(string hash, TorrentSeedConfiguration seedConfiguration, TransmissionSettings settings);
        TransmissionConfig GetConfig(TransmissionSettings settings);
        string GetProtocolVersion(TransmissionSettings settings);
        string GetClientVersion(TransmissionSettings settings);
        void RemoveTorrent(string hash, bool removeData, TransmissionSettings settings);
        void MoveTorrentToTopInQueue(string hashString, TransmissionSettings settings);
    }

    public class TransmissionProxy : ITransmissionProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private ICached<string> _authSessionIDCache;

        public TransmissionProxy(ICacheManager cacheManager, IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _authSessionIDCache = cacheManager.GetCache<string>(GetType(), "authSessionID");
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
            arguments.Add("paused", settings.AddPaused);

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
            arguments.Add("paused", settings.AddPaused);

            if (!downloadDirectory.IsNullOrWhiteSpace())
            {
                arguments.Add("download-dir", downloadDirectory);
            }

            ProcessRequest("torrent-add", arguments, settings);
        }

        public void SetTorrentSeedingConfiguration(string hash, TorrentSeedConfiguration seedConfiguration, TransmissionSettings settings)
        {
            if (seedConfiguration == null)
            {
                return;
            }

            var arguments = new Dictionary<string, object>();
            arguments.Add("ids", new[] { hash });

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

        public string GetProtocolVersion(TransmissionSettings settings)
        {
            var config = GetConfig(settings);

            return config.RpcVersion;
        }

        public string GetClientVersion(TransmissionSettings settings)
        {
            var config = GetConfig(settings);

            return config.Version;
        }

        public TransmissionConfig GetConfig(TransmissionSettings settings)
        {
            // Gets the transmission version.
            var result = GetSessionVariables(settings);

            return Json.Deserialize<TransmissionConfig>(result.Arguments.ToJson());
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
            var fields = new string[]
            {
                "id",
                "hashString", // Unique torrent ID. Use this instead of the client id?
                "name",
                "downloadDir",
                "totalSize",
                "leftUntilDone",
                "isFinished",
                "eta",
                "status",
                "secondsDownloading",
                "secondsSeeding",
                "errorString",
                "uploadedEver",
                "downloadedEver",
                "seedRatioLimit",
                "seedRatioMode",
                "seedIdleLimit",
                "seedIdleMode",
                "fileCount"
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

        private HttpRequestBuilder BuildRequest(TransmissionSettings settings)
        {
            var requestBuilder = new HttpRequestBuilder(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase)
                .Resource("rpc")
                .Accept(HttpAccept.Json);

            requestBuilder.LogResponseContent = true;
            requestBuilder.NetworkCredential = new NetworkCredential(settings.Username, settings.Password);
            requestBuilder.AllowAutoRedirect = false;

            return requestBuilder;
        }

        private void AuthenticateClient(HttpRequestBuilder requestBuilder, TransmissionSettings settings, bool reauthenticate = false)
        {
            var authKey = string.Format("{0}:{1}", requestBuilder.BaseUrl, settings.Password);

            var sessionId = _authSessionIDCache.Find(authKey);

            if (sessionId == null || reauthenticate)
            {
                _authSessionIDCache.Remove(authKey);

                var authLoginRequest = BuildRequest(settings).Build();
                authLoginRequest.SuppressHttpError = true;

                var response = _httpClient.Execute(authLoginRequest);
                if (response.StatusCode == HttpStatusCode.MovedPermanently)
                {
                    var url = response.Headers.GetSingleValue("Location");

                    throw new DownloadClientException("Remote site redirected to " + url);
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    sessionId = response.Headers.GetSingleValue("X-Transmission-Session-Id");

                    if (sessionId == null)
                    {
                        throw new DownloadClientException("Remote host did not return a Session Id.");
                    }
                }
                else
                {
                    throw new DownloadClientAuthenticationException("Failed to authenticate with Transmission.");
                }

                _logger.Debug("Transmission authentication succeeded.");

                _authSessionIDCache.Set(authKey, sessionId);
            }

            requestBuilder.SetHeader("X-Transmission-Session-Id", sessionId);
        }

        public TransmissionResponse ProcessRequest(string action, object arguments, TransmissionSettings settings)
        {
            try
            {
                var requestBuilder = BuildRequest(settings);
                requestBuilder.Headers.ContentType = "application/json";
                requestBuilder.SuppressHttpError = true;

                AuthenticateClient(requestBuilder, settings);

                var request = requestBuilder.Post().Build();

                var data = new Dictionary<string, object>();
                data.Add("method", action);

                if (arguments != null)
                {
                    data.Add("arguments", arguments);
                }

                request.SetContent(data.ToJson());
                request.ContentSummary = string.Format("{0}(...)", action);

                var response = _httpClient.Execute(request);

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    AuthenticateClient(requestBuilder, settings, true);

                    request = requestBuilder.Post().Build();

                    request.SetContent(data.ToJson());
                    request.ContentSummary = string.Format("{0}(...)", action);

                    response = _httpClient.Execute(request);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new DownloadClientAuthenticationException("User authentication failed.");
                }

                var transmissionResponse = Json.Deserialize<TransmissionResponse>(response.Content);

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
            catch (HttpException ex)
            {
                throw new DownloadClientException("Unable to connect to Transmission, please check your settings", ex);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.TrustFailure)
                {
                    throw new DownloadClientUnavailableException("Unable to connect to Transmission, certificate validation failed.", ex);
                }

                throw new DownloadClientUnavailableException("Unable to connect to Transmission, please check your settings", ex);
            }
        }
    }
}
