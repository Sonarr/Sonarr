namespace NzbDrone.Core.MediaFiles
{
    public enum DeleteMediaFileReason
    {
        RemovedFromDisk,
        Manual,
        Upgrade,
        NoLinkedEpisodes
    }
}
