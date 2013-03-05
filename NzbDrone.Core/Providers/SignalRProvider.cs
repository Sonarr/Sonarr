using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Download;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Hubs;
using SignalR;

namespace NzbDrone.Core.Providers
{
    public class SignalRProvider : IHandle<EpisodeGrabbedEvent>
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

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.ParseResult.Episodes)
            {
                UpdateEpisodeStatus(episode.Id, EpisodeStatusType.Downloading, message.ParseResult.Quality);
            }
        }
    }
}
