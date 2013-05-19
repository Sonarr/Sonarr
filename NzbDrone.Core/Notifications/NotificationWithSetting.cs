using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications
{
    public abstract class NotificationWithSetting<TSetting> : NotificationBase where TSetting : class, INotifcationSettings, new()
    {
        public TSetting Settings { get; private set; }

        public TSetting ImportSettingsFromJson(string json)
        {
            Settings = Json.Deserialize<TSetting>(json) ?? new TSetting();

            return Settings;
        }
    }
}
