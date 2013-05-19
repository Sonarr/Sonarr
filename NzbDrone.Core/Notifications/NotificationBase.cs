using System;
using NLog;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public abstract class NotificationBase : INotification
    {
        public abstract string Name { get; }

        public NotificationDefinition InstanceDefinition { get; set; }

        public abstract void OnGrab(string message);
        public abstract void OnDownload(string message, Series series);
        public abstract void AfterRename(Series series);
    }
}
