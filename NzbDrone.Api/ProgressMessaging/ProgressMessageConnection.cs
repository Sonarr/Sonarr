using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using NzbDrone.Api.SignalR;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.ProgressMessaging;

namespace NzbDrone.Api.ProgressMessaging
{
    public class ProgressMessageConnection : NzbDronePersistentConnection,
                                     IHandleAsync<NewProgressMessageEvent>
    {
        public override string Resource
        {
            get { return "/ProgressMessage"; }
        }

        public void HandleAsync(NewProgressMessageEvent message)
        {
            var context = ((ConnectionManager)GlobalHost.ConnectionManager).GetConnection(GetType());
            context.Connection.Broadcast(message.ProgressMessage);
        }
    }
}
