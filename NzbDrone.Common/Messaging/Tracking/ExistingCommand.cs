using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Common.Messaging.Tracking
{
    public class ExistingCommand
    {
        public Boolean Existing { get; set; }
        public TrackedCommand TrackedCommand { get; set; }

        public ExistingCommand(Boolean exisitng, TrackedCommand trackedCommand)
        {
            Existing = exisitng;
            TrackedCommand = trackedCommand;
        }
    }
}
