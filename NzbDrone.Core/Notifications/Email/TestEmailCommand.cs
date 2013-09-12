using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.Email
{
    public class TestEmailCommand : Command
    {
        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public string Server { get; set; }
        public int Port { get; set; }
        public bool Ssl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}
