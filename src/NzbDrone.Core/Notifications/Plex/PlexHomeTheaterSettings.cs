using NzbDrone.Core.Notifications.Xbmc;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexHomeTheaterSettings : XbmcSettings
    {
        public PlexHomeTheaterSettings()
        {
            DisplayTime = 5;
            Port = 3005;
        }
    }
}
