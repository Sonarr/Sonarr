using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Growl
{
    public class Growl : NotificationBase<GrowlSettings>
    {
        private readonly IGrowlService _growlProvider;

        public Growl(IGrowlService growlProvider)
        {
            _growlProvider = growlProvider;
        }

        public override string Name
        {
            get { return "Growl"; }
        }

        public override string ImplementationName
        {
            get { return "Growl"; }
        }

        public override void OnGrab(string message)
        {
            const string title = "Episode Grabbed";

            _growlProvider.SendNotification(title, message, "GRAB", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void OnDownload(string message, Series series)
        {
            const string title = "Episode Downloaded";

            _growlProvider.SendNotification(title, message, "DOWNLOAD", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void AfterRename(Series series)
        {
        }
    }
}
