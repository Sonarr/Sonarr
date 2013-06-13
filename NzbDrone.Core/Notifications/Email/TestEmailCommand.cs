using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Notifications.Email
{
    public class TestEmailCommand : ICommand
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public bool Ssl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}
