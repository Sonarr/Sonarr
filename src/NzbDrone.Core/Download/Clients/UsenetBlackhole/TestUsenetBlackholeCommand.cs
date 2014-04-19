using System;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Download.Clients.UsenetBlackhole
{
    public class TestUsenetBlackholeCommand : Command
    {
        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public String NzbFolder { get; set; }
        public String WatchFolder { get; set; }
    }
}
