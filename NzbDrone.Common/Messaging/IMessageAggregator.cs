namespace NzbDrone.Common.Messaging
{
    /// <summary>
    ///   Enables loosely-coupled publication of events.
    /// </summary>
    public interface IMessageAggregator
    {
        void PublishEvent<TEvent>(TEvent @event) where TEvent : IEvent;
        void PublishCommand<TCommand>(TCommand command) where TCommand : ICommand;
    }
}