using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Notifications.Pushover
{
    public class TestPushoverCommand : ICommand
    {
        public string UserKey { get; set; }
        public int Priority { get; set; }
    }
}
