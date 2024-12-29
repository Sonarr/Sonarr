using Workarr.Messaging;
using Workarr.ThingiProvider.Status;

namespace Workarr.ThingiProvider.Events
{
    public class ProviderStatusChangedEvent<TProvider> : IEvent
    {
        public int ProviderId { get; private set; }

        public ProviderStatusBase Status { get; private set; }

        public ProviderStatusChangedEvent(int id, ProviderStatusBase status)
        {
            ProviderId = id;
            Status = status;
        }
    }
}
