using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public interface INotification : IProvider
    {
        string Link { get; }

        void OnGrab(GrabMessage grabMessage);
        void OnGrabMovie(GrabMovieMessage grabMessage);
        void OnDownload(DownloadMessage message);
        void OnDownloadMovie(DownloadMovieMessage message);
        void OnRename(Series series);
        void OnRenameMovie(Movie series);
        bool SupportsOnGrab { get; }
        bool SupportsOnGrabMovie { get; }
        bool SupportsOnDownload { get; }
        bool SupportsOnDownloadMovie { get; }
        bool SupportsOnUpgrade { get; }
        bool SupportsOnRename { get; }
        bool SupportsOnRenameMovie { get; }
    }
}
