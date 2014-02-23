using System;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Download.Clients.Pneumatic
{
    public class TestPneumaticCommand : Command
    {
        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public String Folder { get; set; }
    }
}
