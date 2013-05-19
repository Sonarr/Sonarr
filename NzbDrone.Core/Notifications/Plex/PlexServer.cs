using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexServer : NotificationWithSetting<PlexServerSettings>
    {
        private readonly PlexProvider _plexProvider;

        public PlexServer(PlexProvider plexProvider)
        {
            _plexProvider = plexProvider;
        }

        public override string Name
        {
            get { return "Plex Server"; }
        }

        public override void OnGrab(string message)
        {
        }

        public override void OnDownload(string message, Series series)
        {
            UpdateIfEnabled();
        }

        public override void AfterRename(Series series)
        {
            UpdateIfEnabled();
        }

        private void UpdateIfEnabled()
        {
            if (Settings.UpdateLibrary)
            {
                _plexProvider.UpdateLibrary(Settings.Host);
            }
        }
    }
}
