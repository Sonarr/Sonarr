using System;
using NzbDrone.Common;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class TestProwlCommand : Command
    {
        public string ApiKey { get; set; }
        public int Priority { get; set; }
    }
}
