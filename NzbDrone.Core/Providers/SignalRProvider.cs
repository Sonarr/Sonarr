using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;
using SignalR;
using SignalR.Hosting.AspNet;
using SignalR.Hubs;
using SignalR.Infrastructure;

namespace NzbDrone.Core.Providers
{
    public class SignalRProvider : Hub
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public virtual void UpdateEpisodeStatus(int episodeId, EpisodeStatusType episodeStatus, QualityModel quality)
        {
            try
            {
                logger.Trace("Sending Status update to client. EpisodeId: {0}, Status: {1}", episodeId, episodeStatus);

                Clients.updatedStatus(new
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
