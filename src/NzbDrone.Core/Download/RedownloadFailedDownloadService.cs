using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Download
{
    public class RedownloadFailedDownloadService : IHandle<DownloadFailedEvent>
    {
        private readonly IConfigService _configService;
        private readonly IEpisodeService _episodeService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;

        public RedownloadFailedDownloadService(IConfigService configService,
                                               IEpisodeService episodeService,
                                               IManageCommandQueue commandQueueManager,
                                               Logger logger)
        {
            _configService = configService;
            _episodeService = episodeService;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        [EventHandleOrder(EventHandleOrder.Last)]
        public void Handle(DownloadFailedEvent message)
        {
            if (!_configService.AutoRedownloadFailed)
            {
                _logger.Debug("Auto redownloading failed episodes is disabled");
                return;
            }

            if (message.EpisodeIds.Count == 1)
            {
                _logger.Debug("Failed download only contains one episode, searching again");

                _commandQueueManager.Push(new EpisodeSearchCommand(message.EpisodeIds));

                return;
            }

            var seasonNumber = _episodeService.GetEpisode(message.EpisodeIds.First()).SeasonNumber;
            var episodesInSeason = _episodeService.GetEpisodesBySeason(message.SeriesId, seasonNumber);

            if (message.EpisodeIds.Count == episodesInSeason.Count)
            {
                _logger.Debug("Failed download was entire season, searching again");

                _commandQueueManager.Push(new SeasonSearchCommand
                {
                    SeriesId = message.SeriesId,
                    SeasonNumber = seasonNumber
                });

                return;
            }

            _logger.Debug("Failed download contains multiple episodes, probably a double episode, searching again");

            _commandQueueManager.Push(new EpisodeSearchCommand(message.EpisodeIds));
        }
    }
}
