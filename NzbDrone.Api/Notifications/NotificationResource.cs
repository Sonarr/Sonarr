using System;
using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Notifications
{
    public class NotificationResource : RestResource
    {
        public String Name { get; set; }
        public String ImplementationName { get; set; }
        public String Link { get; set; }
        public Boolean OnGrab { get; set; }
        public Boolean OnDownload { get; set; }
        public List<Field> Fields { get; set; }
        public String Implementation { get; set; }
        public String TestCommand { get; set; }
    }
}