namespace NzbDrone.Common.Messaging
{
    /// <summary>
    ///   A marker interface for classes that subscribe to messages.
    /// </summary>
    public interface IProcessMessage { }

    /// <summary>
    ///   A marker interface for classes that subscribe to messages.
    /// </summary>
    public interface IProcessMessageAsync : IProcessMessage { }
}