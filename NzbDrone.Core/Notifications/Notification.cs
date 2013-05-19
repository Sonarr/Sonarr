using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Notifications
{
    public class Notification
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool OnGrab { get; set; }
        public bool OnDownload { get; set; }
        public INotifcationSettings Settings { get; set; }
        public INotification Instance { get; set; }
    }
}
