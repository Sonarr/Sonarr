using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class FileStationListFileInfoResponse
    {
        public bool IsDir { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Dictionary<string, object> Additional { get; set; }
    }
}
