using System;

namespace NzbDrone.Core.Messaging.Commands.Tracking
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
