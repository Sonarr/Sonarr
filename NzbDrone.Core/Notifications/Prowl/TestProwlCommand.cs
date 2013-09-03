using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class TestProwlCommand : ICommand
    {
        public String CommandId { get; private set; }
        public string ApiKey { get; set; }
        public int Priority { get; set; }

        public TestProwlCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}
