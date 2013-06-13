using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Notifications.Prowl
{
    public enum ProwlPriority
    {
        VeryLow = -2,
        Low = -1,
        Normal = 0,
        High = 1,
        Emergency = 2
    }
}
