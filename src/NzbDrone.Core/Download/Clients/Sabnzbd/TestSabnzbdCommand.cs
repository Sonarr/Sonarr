using System;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class TestSabnzbdCommand : Command
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
        public String ApiKey { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public Boolean UseSsl { get; set; }
    }
}
