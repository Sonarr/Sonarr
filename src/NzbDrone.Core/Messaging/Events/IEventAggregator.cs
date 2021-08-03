using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Messaging.Events
{
    public interface IEventAggregator
    {
        void PublishEvent<TEvent>(TEvent @event)
            where TEvent : class,  IEvent;
    }
}
