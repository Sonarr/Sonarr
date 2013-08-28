using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Notifications.Plex
{
    public class TestPlexClientCommand : ICommand
    {
        public String CommandId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        
        public TestPlexClientCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}
