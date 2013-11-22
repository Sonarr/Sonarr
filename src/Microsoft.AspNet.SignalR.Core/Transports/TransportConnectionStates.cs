using System;

namespace Microsoft.AspNet.SignalR.Transports
{
    [Flags]
    public enum TransportConnectionStates
    {
        None = 0,
        Added = 1,
        Removed = 2,
        Replaced = 4,
        QueueDrained = 8,
        HttpRequestEnded = 16,
        Disconnected = 32,
        Aborted = 64,
        DisconnectMessageReceived = 128,
        Disposed = 65536,
    }
}
