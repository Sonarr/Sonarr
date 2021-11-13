using System.Collections.Generic;
using System.Net.Http;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Clients.DownloadStation.Responses;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Proxies
{
    public class DownloadStationTaskProxyV1 : DiskStationProxyBase, IDownloadStationTaskProxy
    {
        public DownloadStationTaskProxyV1(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
            : base(DiskStationApi.DownloadStationTask, "SYNO.DownloadStation.Task", httpClient, cacheManager, logger)
        {
        }

        public bool IsApiSupported(DownloadStationSettings settings)
        {
            return GetApiInfo(settings) != null;
        }

        public void AddTaskFromData(byte[] data, string filename, string downloadDirectory, DownloadStationSettings settings)
        {
            var requestBuilder = BuildRequest(settings, "create", 2, HttpMethod.Post);

            if (downloadDirectory.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddFormParameter("destination", downloadDirectory);
            }

            requestBuilder.AddFormUpload("file", filename, data);

            var response = ProcessRequest<object>(requestBuilder, $"add task from data {filename}", settings);
        }

        public void AddTaskFromUrl(string url, string downloadDirectory, DownloadStationSettings settings)
        {
            var requestBuilder = BuildRequest(settings, "create", 3);
            requestBuilder.AddQueryParam("uri", url);

            if (downloadDirectory.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("destination", downloadDirectory);
            }

            var response = ProcessRequest<object>(requestBuilder, $"add task from url {url}", settings);
        }

        public IEnumerable<DownloadStationTask> GetTasks(DownloadStationSettings settings)
        {
            try
            {
                var requestBuilder = BuildRequest(settings, "list", 1);
                requestBuilder.AddQueryParam("additional", "detail,transfer");

                var response = ProcessRequest<DownloadStationTaskInfoResponse>(requestBuilder, "get tasks", settings);

                return response.Data.Tasks;
            }
            catch (DownloadClientException e)
            {
                _logger.Error(e);
                return new List<DownloadStationTask>();
            }
        }

        public void RemoveTask(string downloadId, DownloadStationSettings settings)
        {
            var requestBuilder = BuildRequest(settings, "delete", 1);
            requestBuilder.AddQueryParam("id", downloadId);
            requestBuilder.AddQueryParam("force_complete", false);

            var response = ProcessRequest<object>(requestBuilder, $"remove item {downloadId}", settings);
        }
    }
}
