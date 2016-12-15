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
        bool RemoveTorrent(string downloadId, bool deleteData, DownloadStationSettings settings);
        bool AddTorrentFromUrl(string url, string downloadDirectory, DownloadStationSettings settings);
        bool AddTorrentFromData(byte[] torrentData, string filename, string downloadDirectory, DownloadStationSettings settings);
        IEnumerable<int> GetApiVersion(DownloadStationSettings settings);
    }

    public class DownloadStationProxy : DiskStationProxyBase, IDownloadStationProxy
    {
        public DownloadStationProxy(IHttpClient httpClient, Logger logger)
            : base(httpClient, logger)
        {
        }

        public bool AddTorrentFromData(byte[] torrentData, string filename, string downloadDirectory, DownloadStationSettings settings)
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

            var response = ProcessRequest(DiskStationApi.DownloadStationTask, arguments, settings, HttpMethod.POST);

            return response.Success;
        }

        public bool AddTorrentFromUrl(string torrentUrl, string downloadDirectory, DownloadStationSettings settings)
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

            var response = ProcessRequest(DiskStationApi.DownloadStationTask, arguments, settings, HttpMethod.GET);

            return response.Success;
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

            var response = ProcessRequest<DownloadStationTaskInfoResponse>(DiskStationApi.DownloadStationTask, arguments, settings);

            if (response.Success)
            {
                return response.Data.Tasks.Where(t => t.Type == DownloadStationTaskType.BT);
            }

            return new List<DownloadStationTorrent>();
        }

        public Dictionary<string, object> GetConfig(DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, object>
            {
                { "api", "SYNO.DownloadStation.Info" },
                { "version", "1" },
                { "method", "getconfig" }
            };

            try
            {
                var response = ProcessRequest<Dictionary<string, object>>(DiskStationApi.DownloadStationInfo, arguments, settings);
                return response.Data;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get config from Download Station");

                throw;
            }
        }

        public bool RemoveTorrent(string downloadId, bool deleteData, DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, object>
            {
                { "api", "SYNO.DownloadStation.Task" },
                { "version", "1" },
                { "method", "delete" },
                { "id", downloadId },
                { "force_complete", false }
            };

            try
            {
                var response = ProcessRequest(DiskStationApi.DownloadStationTask, arguments, settings);

                if (response.Success)
                {
                    _logger.Trace("Item {0} removed from Download Station", downloadId);
                }

                return response.Success;
            }
            catch (DownloadClientException e)
            {
                _logger.Debug(e, "Failed to remove item {0} from Download Station", downloadId);

                throw;
            }
        }

        public IEnumerable<int> GetApiVersion(DownloadStationSettings settings)
        {
            return base.GetApiVersion(settings, DiskStationApi.DownloadStationInfo);
        }
    }
}
