using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Model;
using SignalR;
using SignalR.Hosting.AspNet;
using SignalR.Hubs;
using SignalR.Infrastructure;

namespace NzbDrone.Core.Providers
{
    public class SignalRProvider : Hub
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public virtual void UpdateEpisodeStatus(int episodeId, EpisodeStatusType episodeStatus)
        {
            logger.Trace("Sending Status update to client. EpisodeId: {0}, Status: {1}", episodeId, episodeStatus);

            GetClients().updatedStatus(episodeId, episodeStatus.ToString());
        }

        private static dynamic GetClients()
        {
            var connectionManager = AspNetHost.DependencyResolver.Resolve<IConnectionManager>();
            return connectionManager.GetClients<SignalRProvider>();
        }
    }
}
