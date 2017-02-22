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
        IEnumerable<DownloadStationTask> GetTasks(DownloadStationTaskType type, DownloadStationSettings settings);
        Dictionary<string, object> GetConfig(DownloadStationSettings settings);
        void RemoveTask(string downloadId, DownloadStationSettings settings);
        void AddTaskFromUrl(string url, string downloadDirectory, DownloadStationSettings settings);
        void AddTaskFromData(byte[] data, string filename, string downloadDirectory, DownloadStationSettings settings);
        IEnumerable<int> GetApiVersion(DownloadStationSettings settings);
    }

    public class DownloadStationProxy : DiskStationProxyBase, IDownloadStationProxy
    {
        public DownloadStationProxy(IHttpClient httpClient, Logger logger)
            : base(httpClient, logger)
        {
        }

        public void AddTaskFromData(byte[] data, string filename, string downloadDirectory, DownloadStationSettings settings)
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

            arguments.Add("file", new Dictionary<string, object>() { { "name", filename }, { "data", data } });
           
            var response = ProcessRequest(DiskStationApi.DownloadStationTask, arguments, settings, $"add task from data {filename}", HttpMethod.POST);            
        }

        public void AddTaskFromUrl(string url, string downloadDirectory, DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, object>
            {
                { "api", "SYNO.DownloadStation.Task" },
                { "version", "3" },
                { "method", "create" },
                { "uri", url }
            };

            if (downloadDirectory.IsNotNullOrWhiteSpace())
            {
                arguments.Add("destination", downloadDirectory);
            }

            var response = ProcessRequest(DiskStationApi.DownloadStationTask, arguments, settings, $"add task from url {url}");
        }

        public IEnumerable<DownloadStationTask> GetTasks(DownloadStationTaskType type, DownloadStationSettings settings)
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
                var response = ProcessRequest<DownloadStationTaskInfoResponse>(DiskStationApi.DownloadStationTask, arguments, settings, "get tasks");

                return response.Data.Tasks.Where(t => t.Type == type.ToString());
            }
            catch (DownloadClientException e)
            {
                _logger.Error(e);
                return new List<DownloadStationTask>();
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

        public void RemoveTask(string downloadId, DownloadStationSettings settings)
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
