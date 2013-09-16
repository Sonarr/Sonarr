using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.SignalR
{
    public class BroadcastSignalRMessage : Command
    {
        public SignalRMessage Body { get; private set; }

        public BroadcastSignalRMessage(SignalRMessage body)
        {
            Body = body;
        }
    }
}