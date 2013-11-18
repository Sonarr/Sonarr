using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public abstract class NotificationBase<TSettings> : INotification where TSettings : IProviderConfig, new()
    {
        public Type ConfigContract
        {
            get
            {
                return typeof(TSettings);
            }
        }

        public IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                return new List<ProviderDefinition>();
            }
        }

        public ProviderDefinition Definition { get; set; }

        public abstract string Link { get; }

        public abstract void OnGrab(string message);
        public abstract void OnDownload(DownloadMessage message);
        public abstract void AfterRename(Series series);

        protected TSettings Settings
        {
            get
            {
                return (TSettings)Definition.Settings;
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
