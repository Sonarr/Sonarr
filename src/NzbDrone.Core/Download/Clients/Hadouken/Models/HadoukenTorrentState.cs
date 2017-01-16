namespace NzbDrone.Core.Download.Clients.Hadouken.Models
{
    public enum HadoukenTorrentState
    {
        Unknown = 0,
        QueuedForChecking = 1,
        CheckingFiles = 2,
        DownloadingMetadata = 3,
        Downloading = 4,
        Finished = 5,
        Seeding = 6,
        Allocating = 7,
        CheckingResumeData = 8,
        Paused = 9
    }
}
