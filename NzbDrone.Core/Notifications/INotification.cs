using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public interface INotification
    {
        string Name { get; }
        string ImplementationName { get; }
        string Link { get; }

        NotificationDefinition InstanceDefinition { get; set; }

        void OnGrab(string message);
        void OnDownload(string message, Series series);
        void AfterRename(Series series);
    }
}
