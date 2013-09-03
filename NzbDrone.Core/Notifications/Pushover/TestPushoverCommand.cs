using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Notifications.Pushover
{
    public class TestPushoverCommand : ICommand
    {
        public String CommandId { get; private set; }
        public string UserKey { get; set; }
        public int Priority { get; set; }

        public TestPushoverCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}
