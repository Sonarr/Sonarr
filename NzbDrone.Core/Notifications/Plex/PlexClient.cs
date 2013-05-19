using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexClient : NotificationWithSetting<PlexClientSettings>
    {
        private readonly PlexProvider _plexProvider;

        public PlexClient(PlexProvider plexProvider)
        {
            _plexProvider = plexProvider;
        }

        public override string Name
        {
            get { return "Plex Client"; }
        }

        public override void OnGrab(string message)
        {
            const string header = "NzbDrone [TV] - Grabbed";
            _plexProvider.Notify(Settings, header, message);
        }

        public override void OnDownload(string message, Series series)
        {
            const string header = "NzbDrone [TV] - Downloaded";
            _plexProvider.Notify(Settings, header, message);
        }

        public override void AfterRename(Series series)
        {
        }
    }
}
