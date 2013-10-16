using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexServer : NotificationBase<PlexServerSettings>
    {
        private readonly IPlexService _plexProvider;

        public PlexServer(IPlexService plexProvider)
        {
            _plexProvider = plexProvider;
        }

        public override string Link
        {
            get { return "http://www.plexapp.com/"; }
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
                _plexProvider.UpdateLibrary(Settings);
            }
        }
    }
}
