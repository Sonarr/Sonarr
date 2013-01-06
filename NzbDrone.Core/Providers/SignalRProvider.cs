using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Hubs;
using SignalR;
using SignalR.Hubs;

namespace NzbDrone.Core.Providers
{
    public class SignalRProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public virtual void UpdateEpisodeStatus(int episodeId, EpisodeStatusType episodeStatus, QualityModel quality)
        {
            try
            {
                logger.Trace("Sending Status update to client. EpisodeId: {0}, Status: {1}", episodeId, episodeStatus);

                var context = GlobalHost.ConnectionManager.GetHubContext<EpisodeHub>();
                context.Clients.updatedStatus(new
                                               {
                                                       EpisodeId = episodeId,
                                                       EpisodeStatus = episodeStatus.ToString(),
                                                       Quality = (quality == null ? String.Empty : quality.Quality.ToString())
                                               });
            }
            catch (Exception ex)
            {
                logger.TraceException("Error", ex);
                throw;
            }
        }
    }
}
