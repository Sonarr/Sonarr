using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    public enum QBittorrentState
    {
        [FieldOption(Label = "Started")]
        Start = 0,

        [FieldOption(Label = "Force Started")]
        ForceStart = 1,

        [FieldOption(Label = "Stopped")]
        Stop = 2
    }
}
