using System;
using NzbDrone.Common;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.Pushover
{
    public class TestPushoverCommand : Command
    {
        public string UserKey { get; set; }
        public int Priority { get; set; }
    }
}
