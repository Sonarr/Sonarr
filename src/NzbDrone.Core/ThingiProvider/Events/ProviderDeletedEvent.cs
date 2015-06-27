using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.ThingiProvider.Events
{
    public class ProviderDeletedEvent<TProvider> : IEvent
    {
        public int ProviderId { get; private set; }

        public ProviderDeletedEvent(int id)
        {
            ProviderId = id;
        }
    }
}
