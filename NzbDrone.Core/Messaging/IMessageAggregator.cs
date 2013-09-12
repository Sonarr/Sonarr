using NzbDrone.Common.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Messaging
{
    /// <summary>
    ///   Enables loosely-coupled publication of events.
    /// </summary>
    public interface IMessageAggregator
    {
        void PublishEvent<TEvent>(TEvent @event) where TEvent : class,  IEvent;
        void PublishCommand<TCommand>(TCommand command) where TCommand : Command;
        void PublishCommand(string commandTypeName);
        Command PublishCommandAsync<TCommand>(TCommand command) where TCommand : Command;
        Command PublishCommandAsync(string commandTypeName);
    }
}