using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public interface INotification
    {
        string Name { get; }

        NotificationDefinition InstanceDefinition { get; set; }

        void OnGrab(string message);
        void OnDownload(string message, Series series);
        void AfterRename(Series series);
    }
}
