using System;
using System.Collections.Generic;

namespace NzbDrone.Api.Notifications
{
    public class NotificationResource : ProviderResource
    {
        public string Link { get; set; }
        public bool OnGrab { get; set; }
        public bool OnDownload { get; set; }
        public bool OnUpgrade { get; set; }
        public string TestCommand { get; set; }
        public HashSet<int> Tags { get; set; }
    }
}