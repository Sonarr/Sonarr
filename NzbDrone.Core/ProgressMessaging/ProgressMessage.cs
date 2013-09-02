using System;
using NzbDrone.Common.Messaging.Tracking;

namespace NzbDrone.Core.ProgressMessaging
{
    public class ProgressMessage
    {
        public DateTime Time { get; set; }
        public String CommandId { get; set; }
        public String Message { get; set; }
        public ProcessState Status { get; set; }
    }
}
