using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class TestProwlCommand : ICommand
    {
        public string ApiKey { get; set; }
        public int Priority { get; set; }
    }
}
