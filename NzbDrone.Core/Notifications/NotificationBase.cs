using System;
using NLog;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public abstract class NotificationBase<TSetting> : INotification where TSetting : class, INotifcationSettings, new()
    {
        public abstract string Name { get; }

        public NotificationDefinition InstanceDefinition { get; set; }

        public abstract void OnGrab(string message);
        public abstract void OnDownload(string message, Series series);
        public abstract void AfterRename(Series series);

        public TSetting Settings { get; private set; }

        public TSetting ImportSettingsFromJson(string json)
        {
            Settings = Json.Deserialize<TSetting>(json) ?? new TSetting();

            return Settings;
        }
    }
}
