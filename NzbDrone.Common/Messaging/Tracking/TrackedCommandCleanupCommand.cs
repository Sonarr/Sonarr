using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Common.Messaging.Tracking
{
    public class TrackedCommandCleanupCommand : ICommand
    {
        public string CommandId { get; private set; }

        public TrackedCommandCleanupCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}
