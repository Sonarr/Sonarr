using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.Hadouken.Models;

namespace NzbDrone.Core.Download.Clients.Hadouken
{
    public interface IHadoukenProxy
    {
        HadoukenSystemInfo GetSystemInfo(HadoukenSettings settings);
        HadoukenTorrent[] GetTorrents(HadoukenSettings settings);
        IReadOnlyDictionary<string, object> GetConfig(HadoukenSettings settings);
        string AddTorrentFile(HadoukenSettings settings, byte[] fileContent);
        void AddTorrentUri(HadoukenSettings settings, string torrentUrl);
        void RemoveTorrent(HadoukenSettings settings, string downloadId);
        void RemoveTorrentAndData(HadoukenSettings settings, string downloadId);
    }

    public class HadoukenProxy : IHadoukenProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public HadoukenProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public HadoukenSystemInfo GetSystemInfo(HadoukenSettings settings)
        {
            return ProcessRequest<HadoukenSystemInfo>(settings, "core.getSystemInfo");
        }

        public HadoukenTorrent[] GetTorrents(HadoukenSettings settings)
        {
            var result = ProcessRequest<HadoukenTorrentResponse>(settings, "webui.list");

            return GetTorrents(result.Torrents);
        }

        public IReadOnlyDictionary<string, object> GetConfig(HadoukenSettings settings)
        {
            return ProcessRequest<IReadOnlyDictionary<string, object>>(settings, "webui.getSettings");
        }

        public string AddTorrentFile(HadoukenSettings settings, byte[] fileContent)
        {
            return ProcessRequest<string>(settings, "webui.addTorrent", "file", Convert.ToBase64String(fileContent), new { label = settings.Category });
        }

        public void AddTorrentUri(HadoukenSettings settings, string torrentUrl)
        {
            ProcessRequest<string>(settings, "webui.addTorrent", "url", torrentUrl, new { label = settings.Category });
        }

        public void RemoveTorrent(HadoukenSettings settings, string downloadId)
        {
            ProcessRequest<bool>(settings, "webui.perform", "remove", new string[] { downloadId });
        }

        public void RemoveTorrentAndData(HadoukenSettings settings, string downloadId)
        {
            ProcessRequest<bool>(settings, "webui.perform", "removedata", new string[] { downloadId });
        }

        private T ProcessRequest<T>(HadoukenSettings settings, string method, params object[] parameters)
        {
            var baseUrl = HttpRequestBuilder.BuildBaseUrl(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase);
            baseUrl = HttpUri.CombinePath(baseUrl, "api");

            var requestBuilder = new JsonRpcRequestBuilder(baseUrl, method, parameters);
            requestBuilder.LogResponseContent = true;
            requestBuilder.NetworkCredential = new BasicNetworkCredential(settings.Username, settings.Password);
            requestBuilder.Headers.Add("Accept-Encoding", "gzip,deflate");

            var httpRequest = requestBuilder.Build();
            HttpResponse response;

            try
            {
                response = _httpClient.Execute(httpRequest);
            }
            catch (HttpException ex)
            {
                throw new DownloadClientException("Unable to connect to Hadouken, please check your settings", ex);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.TrustFailure)
                {
                    throw new DownloadClientUnavailableException("Unable to connect to Hadouken, certificate validation failed.", ex);
                }

                throw new DownloadClientUnavailableException("Unable to connect to Hadouken, please check your settings", ex);
            }

            var result = Json.Deserialize<JsonRpcResponse<T>>(response.Content);

            if (result.Error != null)
            {
                throw new DownloadClientException("Error response received from Hadouken: {0}", result.Error.ToString());
            }

            return result.Result;
        }

        private HadoukenTorrent[] GetTorrents(object[][] torrentsRaw)
        {
            if (torrentsRaw == null)
            {
                return new HadoukenTorrent[0];
            }

            var torrents = new List<HadoukenTorrent>();

            foreach (var item in torrentsRaw)
            {
                var torrent = MapTorrent(item);
                if (torrent != null)
                {
                    torrent.IsFinished = torrent.Progress >= 1000;
                    torrents.Add(torrent);
                }
            }

            return torrents.ToArray();
        }

        private HadoukenTorrent MapTorrent(object[] item)
        {
            HadoukenTorrent torrent = null;

            try
            {
                torrent = new HadoukenTorrent()
                {
                    InfoHash = Convert.ToString(item[0]),
                    State = ParseState(Convert.ToInt32(item[1])),
                    Name = Convert.ToString(item[2]),
                    TotalSize = Convert.ToInt64(item[3]),
                    Progress = Convert.ToDouble(item[4]),
                    DownloadedBytes = Convert.ToInt64(item[5]),
                    UploadedBytes = Convert.ToInt64(item[6]),
                    DownloadRate = Convert.ToInt64(item[9]),
                    Label = Convert.ToString(item[11]),
                    Error = Convert.ToString(item[21]),
                    SavePath = Convert.ToString(item[26])
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to map Hadouken torrent data.");
            }

            return torrent;
        }

        private HadoukenTorrentState ParseState(int state)
        {
            if ((state & 1) == 1)
            {
                return HadoukenTorrentState.Downloading;
            }
            else if ((state & 2) == 2)
            {
                return HadoukenTorrentState.CheckingFiles;
            }
            else if ((state & 32) == 32)
            {
                return HadoukenTorrentState.Paused;
            }
            else if ((state & 64) == 64)
            {
                return HadoukenTorrentState.QueuedForChecking;
            }

            return HadoukenTorrentState.Unknown;
        }
    }
}
