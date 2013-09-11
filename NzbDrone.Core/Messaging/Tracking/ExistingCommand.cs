using System;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Messaging.Tracking
{
    public class ExistingCommand
    {
        public Boolean Existing { get; set; }
        public Command Command { get; set; }

        public ExistingCommand(Boolean exisitng, Command trackedCommand)
        {
            Existing = exisitng;
            Command = trackedCommand;
        }
    }
}
