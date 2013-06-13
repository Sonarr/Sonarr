using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Notifications.Growl
{
    public class TestGrowlCommand : ICommand
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
    }
}
