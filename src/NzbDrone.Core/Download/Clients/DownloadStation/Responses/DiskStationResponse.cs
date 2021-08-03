namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DiskStationResponse<T>
        where T : new()
    {
        public bool Success { get; set; }

        public DiskStationError Error { get; set; }

        public T Data { get; set; }
    }
}
