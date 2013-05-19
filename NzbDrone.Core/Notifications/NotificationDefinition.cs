using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Notifications
{
    public class NotificationDefinition : ModelBase
    {
        public String Name { get; set; }
        public Boolean OnGrab { get; set; }
        public Boolean OnDownload { get; set; }
        public String Settings { get; set; }
        public String Implementation { get; set; }
    }
}