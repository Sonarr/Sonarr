using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Clients.DownloadStation.Responses;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Proxies
{
    public interface IDownloadStationProxy
    {
        IEnumerable<DownloadStationTorrent> GetTorrents(DownloadStationSettings settings);
        Dictionary<string, object> GetConfig(DownloadStationSettings settings);
        void RemoveTorrent(string downloadId, DownloadStationSettings settings);
        void AddTorrentFromUrl(string url, string downloadDirectory, DownloadStationSettings settings);
        void AddTorrentFromData(byte[] torrentData, string filename, string downloadDirectory, DownloadStationSettings settings);
        IEnumerable<int> GetApiVersion(DownloadStationSettings settings);
    }

    public class DownloadStationProxy : DiskStationProxyBase, IDownloadStationProxy
    {
        public DownloadStationProxy(IHttpClient httpClient, Logger logger)
            : base(httpClient, logger)
        {
        }

        public void AddTorrentFromData(byte[] torrentData, string filename, string downloadDirectory, DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, object>
            {
                { "api", "SYNO.DownloadStation.Task" },
                { "version", "2" },
                { "method", "create" }
            };

            if (downloadDirectory.IsNotNullOrWhiteSpace())
            {
                arguments.Add("destination", downloadDirectory);
            }

            arguments.Add("file", new Dictionary<string, object>() { { "name", filename }, { "data", torrentData } });
           
            var response = ProcessRequest(DiskStationApi.DownloadStationTask, arguments, settings, $"add torrent from data {filename}", HttpMethod.POST);            
        }

        public void AddTorrentFromUrl(string torrentUrl, string downloadDirectory, DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, object>
            {
                { "api", "SYNO.DownloadStation.Task" },
                { "version", "3" },
                { "method", "create" },
                { "uri", torrentUrl }
            };

            if (downloadDirectory.IsNotNullOrWhiteSpace())
            {
                arguments.Add("destination", downloadDirectory);
            }

            var response = ProcessRequest(DiskStationApi.DownloadStationTask, arguments, settings, $"add torrent from url {torrentUrl}", HttpMethod.GET);
        }

        public IEnumerable<DownloadStationTorrent> GetTorrents(DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, object>
            {
                 { "api", "SYNO.DownloadStation.Task" },
                 { "version", "1" },
                 { "method", "list" },
                 { "additional", "detail,transfer" }
            };

            try
            {
                var response = ProcessRequest<DownloadStationTaskInfoResponse>(DiskStationApi.DownloadStationTask, arguments, settings, "get torrents");

                return response.Data.Tasks.Where(t => t.Type == DownloadStationTaskType.BT);
            }
            catch (DownloadClientException e)
            {
                _logger.Error(e);
                return new List<DownloadStationTorrent>();
            }
        }

        public Dictionary<string, object> GetConfig(DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, object>
            {
                { "api", "SYNO.DownloadStation.Info" },
                { "version", "1" },
                { "method", "getconfig" }
            };

            var response = ProcessRequest<Dictionary<string, object>>(DiskStationApi.DownloadStationInfo, arguments, settings, "get config");

            return response.Data;
        }

        public void RemoveTorrent(string downloadId, DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, object>
            {
                { "api", "SYNO.DownloadStation.Task" },
                { "version", "1" },
                { "method", "delete" },
                { "id", downloadId },
                { "force_complete", false }
            };

            var response = ProcessRequest(DiskStationApi.DownloadStationTask, arguments, settings, $"remove item {downloadId}");
        }

        public IEnumerable<int> GetApiVersion(DownloadStationSettings settings)
        {
            return base.GetApiVersion(settings, DiskStationApi.DownloadStationInfo);
        }
    }
}
