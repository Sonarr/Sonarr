using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public interface INotification : IProvider
    {
        string Link { get; }

        void OnGrab(string message);
        void OnDownload(string message, Series series);
        void AfterRename(Series series);
    }
}
