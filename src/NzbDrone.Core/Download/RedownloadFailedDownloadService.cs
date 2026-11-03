using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
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
            if (message.SkipRedownload)
            {
                _logger.Debug("Skip redownloading requested by user");
                return;
            }

            if (!_configService.AutoRedownloadFailed)
            {
                _logger.Debug("Auto redownloading failed episodes is disabled");
                return;
            }

            if (message.ReleaseSource == ReleaseSourceType.InteractiveSearch && !_configService.AutoRedownloadFailedFromInteractiveSearch)
            {
                _logger.Debug("Auto redownloading failed episodes from interactive search is disabled");
                return;
            }

            if (message.EpisodeIds.Count == 1)
            {
                _logger.Debug("Failed download only contains one episode, searching again");

                _commandQueueManager.Push(new EpisodeSearchCommand(message.EpisodeIds));

                return;
            }

            var remainingEpisodeIds = new List<int>();

            foreach (var season in _episodeService.GetEpisodes(message.EpisodeIds).GroupBy(e => e.SeasonNumber))
            {
                var episodesInSeason = _episodeService.GetEpisodesBySeason(message.SeriesId, season.Key);

                if (season.Count() == episodesInSeason.Count)
                {
                    _logger.Debug("Failed download contains entire season {0}, searching again", season.Key);

                    _commandQueueManager.Push(new SeasonSearchCommand
                    {
                        SeriesId = message.SeriesId,
                        SeasonNumber = season.Key
                    });

                    continue;
                }

                remainingEpisodeIds.AddRange(season.Select(e => e.Id));
            }

            if (remainingEpisodeIds.Any())
            {
                _logger.Debug("Failed download contains multiple episodes, probably a double episode, searching again");

                _commandQueueManager.Push(new EpisodeSearchCommand(remainingEpisodeIds));
            }
        }
    }
}
