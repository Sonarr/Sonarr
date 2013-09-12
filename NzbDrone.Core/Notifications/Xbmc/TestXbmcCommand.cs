using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class TestXbmcCommand : Command
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
        public int DisplayTime { get; set; }
    }
}
