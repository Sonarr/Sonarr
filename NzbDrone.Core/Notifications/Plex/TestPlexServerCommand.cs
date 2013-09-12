using System;
using NzbDrone.Common;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.Plex
{
    public class TestPlexServerCommand : Command
    {
        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public string Host { get; set; }
        public int Port { get; set; }
    }
}
