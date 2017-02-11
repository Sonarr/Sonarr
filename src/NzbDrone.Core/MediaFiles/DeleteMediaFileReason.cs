namespace NzbDrone.Core.MediaFiles
{
    public enum DeleteMediaFileReason
    {
        MissingFromDisk,
        Manual,
        Upgrade,
        NoLinkedEpisodes,
        ManualOverride
    }
}
