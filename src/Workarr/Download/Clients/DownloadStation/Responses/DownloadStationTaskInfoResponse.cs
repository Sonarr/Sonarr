namespace Workarr.Download.Clients.DownloadStation.Responses
{
    public class DownloadStationTaskInfoResponse
    {
        public int Offset { get; set; }
        public List<DownloadStationTask> Tasks { get; set; }
        public int Total { get; set; }
    }
}
