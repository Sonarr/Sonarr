namespace NzbDrone.Core.Download.Clients.Transmission
{
    public enum TransmissionTorrentStatus
    {
        Stopped = 0,
        CheckWait = 1,
        Check = 2,
        Queued = 3,
        Downloading = 4,
        SeedingWait = 5,
        Seeding = 6
    }
}
