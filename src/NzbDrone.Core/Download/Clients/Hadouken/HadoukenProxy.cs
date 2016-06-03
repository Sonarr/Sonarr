using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using NzbDrone.Core.Download.Clients.Hadouken.Models;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Download.Clients.Hadouken
{
    public class HadoukenProxy : IHadoukenProxy
    {
        private static int _callId;
        private readonly Logger _logger;

        public HadoukenProxy(Logger logger)
        {
            _logger = logger;
        }

        public HadoukenSystemInfo GetSystemInfo(HadoukenSettings settings)
        {
            return ProcessRequest<HadoukenSystemInfo>(settings, "core.getSystemInfo").Result;
        }

        public HadoukenTorrent[] GetTorrents(HadoukenSettings settings)
        {
            var result = ProcessRequest<HadoukenResponseResult>(settings, "webui.list").Result;
            
            return GetTorrents(result.Torrents);
        }

        public IDictionary<string, object> GetConfig(HadoukenSettings settings)
        {
            return ProcessRequest<IDictionary<string, object>>(settings, "webui.getSettings").Result;
        }

        public string AddTorrentFile(HadoukenSettings settings, byte[] fileContent)
        {
            return ProcessRequest<string>(settings, "webui.addTorrent", "file", Convert.ToBase64String(fileContent)).Result;
        }

        public void AddTorrentUri(HadoukenSettings settings, string torrentUrl)
        {
            ProcessRequest<string>(settings, "webui.addTorrent", "url", torrentUrl);
        }

        public void RemoveTorrent(HadoukenSettings settings, string downloadId)
        {
            ProcessRequest<bool>(settings, "webui.perform", "remove", new string[] { downloadId });
        }

        public void RemoveTorrentAndData(HadoukenSettings settings, string downloadId)
        {
            ProcessRequest<bool>(settings, "webui.perform", "removedata", new string[] { downloadId });
        }

        private HadoukenResponse<TResult> ProcessRequest<TResult>(HadoukenSettings settings, string method, params object[] parameters)
        {
            var client = BuildClient(settings);
            return ProcessRequest<TResult>(client, method, parameters);
        }

        private HadoukenResponse<TResult> ProcessRequest<TResult>(IRestClient client, string method, params object[] parameters)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "api";
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Accept-Encoding", "gzip,deflate");

            var data = new Dictionary<String, Object>();
            data.Add("id", GetCallId());
            data.Add("method", method);

            if (parameters != null)
            {
                data.Add("params", parameters);
            }

            request.AddBody(data);

            _logger.Debug("Url: {0} Method: {1}", client.BuildUri(request), method);
            return client.ExecuteAndValidate<HadoukenResponse<TResult>>(request);
        }

        private IRestClient BuildClient(HadoukenSettings settings)
        {
            var protocol = settings.UseSsl ? "https" : "http";
            var url = string.Format(@"{0}://{1}:{2}", protocol, settings.Host, settings.Port);

            var restClient = RestClientFactory.BuildClient(url);
            restClient.Timeout = 4000;

            var basicData = Encoding.UTF8.GetBytes(string.Format("{0}:{1}", settings.Username, settings.Password));
            var basicHeader = Convert.ToBase64String(basicData);

            restClient.AddDefaultHeader("Authorization", string.Format("Basic {0}", basicHeader));

            return restClient;
        }

        private int GetCallId()
        {
            return System.Threading.Interlocked.Increment(ref _callId);
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
                    DownloadRate = Convert.ToInt64(item[9]),
                    Error = Convert.ToString(item[21]),
                    SavePath = Convert.ToString(item[26])
                };
            }
            catch(Exception ex)
            {
                _logger.ErrorException("Failed to map Hadouken torrent data.", ex);
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