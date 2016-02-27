using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NzbDrone.Common.Disk;
using NzbDrone.Core.RemotePathMappings;
using System.IO;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DownloadStationTask
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public long Size { get; set; }

        [JsonProperty(PropertyName = "status_extra")]
        public DownloadStationTaskStatusExtra StatusExtra { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DownloadStationTaskStatus Status { get; set; }

        public DownloadStationTaskAdditional Additional { get; set; }

        public DownloadClientItem ToDownloadClientItem(RemotePathMappingService remotePathMappingService, DownloadStationSettings settings)
        {
            var path = new OsPath(Path.Combine(Additional.Detail.Destination, Title));

            return new DownloadClientItem
            {
                DownloadClient = "Download Station",
                DownloadId = Id,
                Title = Title,
                TotalSize = Size,
                RemainingSize = Size - Additional.Transfer.Downloaded,
                OutputPath = remotePathMappingService.RemapRemoteToLocal(settings.Host, path),
                Message = GetMessage(),
                Status = GetStatus()
            };
        }

        private string GetMessage()
        {
            if (StatusExtra != null)
            {
                if (Status == DownloadStationTaskStatus.Extracting)
                {
                    return string.Format("Extracting: %{0}", StatusExtra.UnzipProgress);
                }

                if (Status == DownloadStationTaskStatus.Error)
                {
                    return StatusExtra.ErrorDetail;
                }
            }

            return null;
        }

        private DownloadItemStatus GetStatus()
        {
            switch (Status)
            {
                case DownloadStationTaskStatus.Waiting:
                    return DownloadItemStatus.Queued;
                case DownloadStationTaskStatus.Paused:
                    return DownloadItemStatus.Paused;
                case DownloadStationTaskStatus.Finished:
                case DownloadStationTaskStatus.Seeding:
                    return DownloadItemStatus.Completed;
                case DownloadStationTaskStatus.Error:
                    return DownloadItemStatus.Failed;
            }

            return DownloadItemStatus.Downloading;
        }
    }
}