namespace NzbDrone.Core.Download.Clients.Putio
{
    public enum PutioTorrentStatus
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
