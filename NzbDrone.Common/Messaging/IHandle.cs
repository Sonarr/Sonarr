namespace NzbDrone.Common.Messaging
{
    /// <summary>
    ///   Denotes a class which can handle a particular type of message.
    /// </summary>
    /// <typeparam name = "TEvent">The type of message to handle.</typeparam>
    public interface IHandle<TEvent> : IProcessMessage where TEvent : IEvent
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
    public interface IHandleAsync<TEvent> : IProcessMessageAsync where TEvent : IEvent
    {
        /// <summary>
        ///   Handles the message asynchronously.
        /// </summary>
        /// <param name = "message">The message.</param>
        void HandleAsync(TEvent message);
    }
}