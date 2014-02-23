using System;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class TestNzbgetCommand : Command
    {
        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public String Host { get; set; }
        public Int32 Port { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
    }
}
