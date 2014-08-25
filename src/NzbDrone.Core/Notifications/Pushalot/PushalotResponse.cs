using System;

namespace NzbDrone.Core.Notifications.Pushalot
{
    public class PushalotResponse
    {
        public Boolean Success { get; set; }
        public Int32 Status { get; set; }
        public String Description { get; set; }
    }
}
