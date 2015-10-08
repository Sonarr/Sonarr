using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Notifications
{
    public interface INotification : IProvider
    {
        string Link { get; }

        void OnGrab(GrabMessage grabMessage);
        void OnDownload(DownloadMessage message);
        void OnRename(Series series);
        void OnUpdateAvailable(UpdatePackage updatePackage);

        bool SupportsOnGrab { get; }
        bool SupportsOnDownload { get; }
        bool SupportsOnUpgrade { get; }
        bool SupportsOnRename { get; }
        bool SupportsOnUpdateAvailable { get; }
    }
}
