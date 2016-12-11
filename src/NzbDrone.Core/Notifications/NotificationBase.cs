using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public abstract class NotificationBase<TSettings> : INotification where TSettings : IProviderConfig, new()
    {
        public abstract string Name { get; }

        public Type ConfigContract
        {
            get
            {
                return typeof(TSettings);
            }
        }

        public virtual ProviderMessage Message
        {
            get
            {
                return null;
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
        public abstract ValidationResult Test();

        public abstract string Link { get; }

        public abstract void OnGrab(GrabMessage grabMessage);
        public abstract void OnDownload(DownloadMessage message); 
        public abstract void OnRename(Series series);

        public virtual bool SupportsOnGrab { get { return true; } }
        public virtual bool SupportsOnDownload { get { return true; } }
        public virtual bool SupportsOnUpgrade { get { return true; } }
        public virtual bool SupportsOnRename { get { return true; } }

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

        public virtual object RequestAction(string action, IDictionary<string, string> query) { return null; }

    }
}
