using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.Plex
{
    public class TestPlexClientCommand : Command
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
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
