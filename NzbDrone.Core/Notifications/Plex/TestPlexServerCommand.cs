using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Notifications.Plex
{
    public class TestPlexServerCommand : ICommand
    {
        public String CommandId { get; private set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public TestPlexServerCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}
