using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.SkyhookNotifications
{
    public class SkyhookNotification
    {
        public int Id { get; set; }
        public string MinimumVersion { get; set; }
        public string MaximumVersion { get; set; }
        public SkyhookNotificationType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string RegexMatch { get; set; }
        public string RegexReplace { get; set; }
    }
}
