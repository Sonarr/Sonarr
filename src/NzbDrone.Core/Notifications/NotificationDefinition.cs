using System;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public class NotificationDefinition : ProviderDefinition
    {
        public Boolean OnGrab { get; set; }
        public Boolean OnDownload { get; set; }
        public Boolean OnUpgrade { get; set; }
    }
}