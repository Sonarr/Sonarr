namespace NzbDrone.Common.Messaging
{
    /// <summary>
    ///   Enables loosely-coupled publication of events.
    /// </summary>
    public interface IMessageAggregator
    {
        void Publish<TEvent>(TEvent message) where TEvent : IEvent;
        void Execute<TCommand>(TCommand message) where TCommand : ICommand;
    }
}