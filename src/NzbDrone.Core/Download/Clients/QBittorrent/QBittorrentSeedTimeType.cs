using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    public enum QBittorrentSeedTimeType
    {
        [FieldOption(Label = "Total")]
        Total = 0,

        [FieldOption(Label = "Inactive")]
        Inactive = 1
    }
}
