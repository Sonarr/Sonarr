namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public enum DownloadStationTaskStatus
    {
        Waiting,
        Downloading,
        Paused,
        Finishing,
        Finished,
        HashChecking,
        Seeding,
        FileHostingWaiting,
        Extracting,
        Error
    }
}
