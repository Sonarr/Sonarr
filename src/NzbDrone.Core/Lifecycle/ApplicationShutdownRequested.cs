using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Lifecycle
{
    public class ApplicationShutdownRequested : IEvent
    {
        public bool Restarting { get; set; }

        public ApplicationShutdownRequested()
        {
        }

        public ApplicationShutdownRequested(bool restarting)
        {
            Restarting = restarting;
        }
    }
}