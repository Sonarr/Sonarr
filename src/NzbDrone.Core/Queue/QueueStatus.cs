namespace NzbDrone.Core.Queue
{
    public enum QueueStatus
    {
        Unknown,
        Queued,
        Paused,
        Downloading,
        Completed,
        Failed,
        Warning,
        Delay,
        DownloadClientUnavailable,
        Fallback
    }
}
