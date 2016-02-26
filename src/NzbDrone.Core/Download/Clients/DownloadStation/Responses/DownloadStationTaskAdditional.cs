namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DownloadStationTaskAdditional
    {
        public DownloadStationTaskDetail Detail { get; set; }

        public DownloadStationTaskTransfer Transfer { get; }
    }
}
