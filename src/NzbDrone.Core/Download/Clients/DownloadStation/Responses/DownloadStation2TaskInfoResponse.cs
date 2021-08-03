using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DownloadStation2TaskInfoResponse
    {
        public int Offset { get; set; }
        public List<DownloadStation2Task> Task { get; set; }
        public int Total { get; set; }
    }
}
