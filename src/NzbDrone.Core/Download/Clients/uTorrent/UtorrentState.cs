namespace NzbDrone.Core.Download.Clients.UTorrent
{
    public enum UTorrentState
    {
        Start = 0,
        ForceStart = 1,
        Pause = 2,
        Stop = 3
    }
}
