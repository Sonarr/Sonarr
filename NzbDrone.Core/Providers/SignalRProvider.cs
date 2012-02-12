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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public virtual void UpdateEpisodeStatus(int episodeId, EpisodeStatusType episodeStatus)
        {
            Logger.Trace("Sending Status update to client. EpisodeId: {0}, Status: {1}", episodeId, episodeStatus);

            GetClients().updatedStatus(episodeId, episodeStatus.ToString());
        }

        private static dynamic GetClients()
        {
            IConnectionManager connectionManager = AspNetHost.DependencyResolver.Resolve<IConnectionManager>();
            return connectionManager.GetClients<SignalRProvider>();
        }
    }
}
