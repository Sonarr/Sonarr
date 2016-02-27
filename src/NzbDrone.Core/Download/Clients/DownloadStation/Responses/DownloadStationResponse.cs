using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DownloadStationResponse<T>
    {
        public bool Success { get; set; }

        public DownloadStationError Error { get; set; }

        public T Data { get; set; }
    }
}
