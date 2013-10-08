using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.ThingiProvider;

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


    public class NotificationProviderModel : ProviderDefinition
    {
        public Boolean OnGrab { get; set; }
        public Boolean OnDownload { get; set; }
    }
}