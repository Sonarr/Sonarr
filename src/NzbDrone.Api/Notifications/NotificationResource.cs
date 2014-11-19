using System;
using System.Collections.Generic;

namespace NzbDrone.Api.Notifications
{
    public class NotificationResource : ProviderResource
    {
        public String Link { get; set; }
        public Boolean OnGrab { get; set; }
        public Boolean OnDownload { get; set; }
        public Boolean OnUpgrade { get; set; }
        public String TestCommand { get; set; }
        public HashSet<Int32> Tags { get; set; }
    }
}