using System;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Download.Clients.Blackhole
{
    public class TestBlackholeCommand : Command
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
