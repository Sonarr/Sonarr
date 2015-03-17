using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Messaging.Commands
{
    public enum CommandPriority
    {
        Low = -1,
        Normal = 0,
        High = 1
    }
}
