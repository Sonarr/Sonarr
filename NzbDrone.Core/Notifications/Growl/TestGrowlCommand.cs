using System;
using NzbDrone.Common;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.Growl
{
    public class TestGrowlCommand : Command
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
    }
}
