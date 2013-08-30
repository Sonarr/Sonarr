using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.ProgressMessaging
{
    public class ProgressMessage
    {
        public DateTime Time { get; set; }
        public String CommandId { get; set; }
        public String Message { get; set; }
    }
}
