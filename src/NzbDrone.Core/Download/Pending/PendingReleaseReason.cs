namespace NzbDrone.Core.Download.Pending
{
    public enum PendingReleaseReason
    {
        Delay = 0,
        DownloadClientUnavailable = 1,
        Fallback = 2
    }
}
