using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class FileStationListResponse
    {
        public List<FileStationListFileInfoResponse> Files { get; set; }
    }
}
