using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Notifications.Growl
{
    public class TestGrowlCommand : ICommand
    {
        public String CommandId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }

        public TestGrowlCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}
