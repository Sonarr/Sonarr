using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using NzbDrone.Api.SignalR;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Episodes
{
    public class EpisodeConnection : BasicResourceConnection<Episode>, IHandleAsync<EpisodeGrabbedEvent>
    {
        public override string Resource
        {
            get { return "/Episodes"; }
        }

        public void HandleAsync(EpisodeGrabbedEvent message)
        {
            var context = ((ConnectionManager)GlobalHost.ConnectionManager).GetConnection(GetType());
            context.Connection.Broadcast(message);
        }
    }
}
