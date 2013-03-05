using System.Linq;

namespace NzbDrone.Common.Eventing
{
    /// <summary>
    ///   Denotes a class which can handle a particular type of message.
    /// </summary>
    /// <typeparam name = "TEvent">The type of message to handle.</typeparam>
    public interface IHandle<TEvent> : IHandle where TEvent : IEvent
    {
        /// <summary>
        ///   Handles the message synchronously.
        /// </summary>
        /// <param name = "message">The message.</param>
        void Handle(TEvent message);
    }

    /// <summary>
    ///   Denotes a class which can handle a particular type of message.
    /// </summary>
    /// <typeparam name = "TEvent">The type of message to handle.</typeparam>
    public interface IHandleAsync<TEvent> : IHandleAsync where TEvent : IEvent
    {
        /// <summary>
        ///   Handles the message asynchronously.
        /// </summary>
        /// <param name = "message">The message.</param>
        void HandleAsync(TEvent message);
    }

    /// <summary>
    ///   A marker interface for classes that subscribe to messages.
    /// </summary>
    public interface IHandle { }

    /// <summary>
    ///   A marker interface for classes that subscribe to messages.
    /// </summary>
    public interface IHandleAsync : IHandle { }
}