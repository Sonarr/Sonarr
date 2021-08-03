using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.ThingiProvider.Events
{
    public class ProviderUpdatedEvent<TProvider> : IEvent
    {
        public ProviderDefinition Definition { get; private set; }

        public ProviderUpdatedEvent(ProviderDefinition definition)
        {
            Definition = definition;
        }
    }
}
