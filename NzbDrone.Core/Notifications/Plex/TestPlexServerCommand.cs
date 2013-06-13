using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Notifications.Plex
{
    public class TestPlexServerCommand : ICommand
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
