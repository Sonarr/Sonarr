using System;
using NzbDrone.Common;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Instrumentation.Commands
{
    public class ClearLogCommand : Command
    {
        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }
    }
}