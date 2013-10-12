using System;

namespace NzbDrone.Api.Notifications
{
    public class NotificationResource : ProviderResource
    {
        public String Link { get; set; }
        public Boolean OnGrab { get; set; }
        public Boolean OnDownload { get; set; }
        public String TestCommand { get; set; }
    }
}