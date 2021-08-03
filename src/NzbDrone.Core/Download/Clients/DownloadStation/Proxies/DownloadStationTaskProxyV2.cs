using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Clients.DownloadStation.Responses;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Proxies
{
    public class DownloadStationTaskProxyV2 : DiskStationProxyBase, IDownloadStationTaskProxy
    {
        public DownloadStationTaskProxyV2(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
            : base(DiskStationApi.DownloadStation2Task, "SYNO.DownloadStation2.Task", httpClient, cacheManager, logger)
        {
        }

        public bool IsApiSupported(DownloadStationSettings settings)
        {
            return GetApiInfo(settings) != null;
        }

        public void AddTaskFromData(byte[] data, string filename, string downloadDirectory, DownloadStationSettings settings)
        {
            var requestBuilder = BuildRequest(settings, "create", 2, HttpMethod.POST);

            requestBuilder.AddFormParameter("type", "\"file\"");
            requestBuilder.AddFormParameter("file", "[\"fileData\"]");
            requestBuilder.AddFormParameter("create_list", "false");

            if (downloadDirectory.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddFormParameter("destination", $"\"{downloadDirectory}\"");
            }

            requestBuilder.AddFormUpload("fileData", filename, data);

            ProcessRequest<object>(requestBuilder, $"add task from data {filename}", settings);
        }

        public void AddTaskFromUrl(string url, string downloadDirectory, DownloadStationSettings settings)
        {
            var requestBuilder = BuildRequest(settings, "create", 2);

            requestBuilder.AddQueryParam("type", "url");
            requestBuilder.AddQueryParam("url", url);
            requestBuilder.AddQueryParam("create_list", "false");

            if (downloadDirectory.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("destination", downloadDirectory);
            }

            ProcessRequest<object>(requestBuilder, $"add task from url {url}", settings);
        }

        public IEnumerable<DownloadStationTask> GetTasks(DownloadStationSettings settings)
        {
            try
            {
                var result = new List<DownloadStationTask>();

                var requestBuilder = BuildRequest(settings, "list", 1);
                requestBuilder.AddQueryParam("additional", "detail");

                var response = ProcessRequest<DownloadStation2TaskInfoResponse>(requestBuilder, "get tasks with additional detail", settings);

                if (response.Success && response.Data.Total > 0)
                {
                    requestBuilder.AddQueryParam("additional", "transfer");
                    var responseTransfer = ProcessRequest<DownloadStation2TaskInfoResponse>(requestBuilder, "get tasks with additional transfer", settings);

                    if (responseTransfer.Success)
                    {
                        foreach (var task in response.Data.Task)
                        {
                            var taskTransfer = responseTransfer.Data.Task.Where(t => t.Id == task.Id).First();

                            var combinedTask = new DownloadStationTask
                            {
                                Username = task.Username,
                                Id = task.Id,
                                Title = task.Title,
                                Size = task.Size,
                                Status = (DownloadStationTaskStatus)task.Status,
                                Type = task.Type,
                                Additional = new DownloadStationTaskAdditional
                                {
                                    Detail = task.Additional.Detail,
                                    Transfer = taskTransfer.Additional.Transfer
                                }
                            };

                            result.Add(combinedTask);
                        }
                    }
                }

                return result;
            }
            catch (DownloadClientException e)
            {
                _logger.Error(e);
                return new List<DownloadStationTask>();
            }
        }

        public void RemoveTask(string downloadId, DownloadStationSettings settings)
        {
            var requestBuilder = BuildRequest(settings, "delete", 2);
            requestBuilder.AddQueryParam("id", downloadId);
            requestBuilder.AddQueryParam("force_complete", "false");

            ProcessRequest<object>(requestBuilder, $"remove item {downloadId}", settings);
        }
    }
}
