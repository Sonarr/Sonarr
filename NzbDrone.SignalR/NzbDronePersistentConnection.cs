using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.SignalR
{
    public sealed class NzbDronePersistentConnection : PersistentConnection, IExecute<BroadcastSignalRMessage>
    {
        private IPersistentConnectionContext Context
        {
            get
            {
                return ((ConnectionManager)GlobalHost.ConnectionManager).GetConnection(GetType());
            }
        }


        public void Execute(BroadcastSignalRMessage message)
        {
            Context.Connection.Broadcast(message.Body);
        }
    }
}