using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using NzbDrone.Api.SignalR;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Messaging.Events;
using NzbDrone.Common.Messaging.Tracking;

namespace NzbDrone.Api.Commands
{
    public class CommandConnection : NzbDronePersistentConnection,
                                     IHandleAsync<CommandStartedEvent>,
                                     IHandleAsync<CommandCompletedEvent>,
                                     IHandleAsync<CommandFailedEvent>
    {
        public override string Resource
        {
            get { return "/Command"; }
        }

        public void HandleAsync(CommandStartedEvent message)
        {
            BroadcastMessage(message.TrackedCommand);
        }

        public void HandleAsync(CommandCompletedEvent message)
        {
            BroadcastMessage(message.TrackedCommand);
        }

        public void HandleAsync(CommandFailedEvent message)
        {
            BroadcastMessage(message.TrackedCommand);
        }

        private void BroadcastMessage(TrackedCommand trackedCommand)
        {
            var context = ((ConnectionManager)GlobalHost.ConnectionManager).GetConnection(GetType());
            context.Connection.Broadcast(trackedCommand);
        }
    }
}
