namespace NzbDrone.SignalR
{
    public class BroadcastSignalRMessage : Core.Messaging.Commands.Command
    {
        public SignalRMessage Body { get; private set; }

        public BroadcastSignalRMessage(SignalRMessage body)
        {
            Body = body;
        }
    }
}