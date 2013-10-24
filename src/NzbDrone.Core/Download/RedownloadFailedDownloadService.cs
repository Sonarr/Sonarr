using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Download
{
    public interface IRedownloadFailedDownloads
    {
        void Redownload(int seriesId, List<int> episodeIds);
    }

    public class RedownloadFailedDownloadService : IRedownloadFailedDownloads
    {
        private readonly IConfigService _configService;
        private readonly IEpisodeService _episodeService;
        private readonly ICommandExecutor _commandExecutor;
        private readonly Logger _logger;

        public RedownloadFailedDownloadService(IConfigService configService, IEpisodeService episodeService, ICommandExecutor commandExecutor, Logger logger)
        {
            _configService = configService;
            _episodeService = episodeService;
            _commandExecutor = commandExecutor;
            _logger = logger;
        }

        public void Redownload(int seriesId, List<int> episodeIds)
        {
            if (!_configService.AutoRedownloadFailed)
            {
                _logger.Trace("Auto redownloading failed episodes is disabled");
                return;
            }

            if (episodeIds.Count == 1)
            {
                _logger.Trace("Failed download only contains one episode, searching again");

                _commandExecutor.PublishCommandAsync(new EpisodeSearchCommand
                                                     {
                                                         EpisodeIds = episodeIds.ToList()
                                                     });

                return;
            }

            var seasonNumber = _episodeService.GetEpisode(episodeIds.First()).SeasonNumber;
            var episodesInSeason = _episodeService.GetEpisodesBySeason(seriesId, seasonNumber);

            if (episodeIds.Count == episodesInSeason.Count)
            {
                _logger.Trace("Failed download was entire season, searching again");

                _commandExecutor.PublishCommandAsync(new SeasonSearchCommand
                                                     {
                                                         SeriesId = seriesId,
                                                         SeasonNumber = seasonNumber
                                                     });

                return;
            }

            _logger.Trace("Failed download contains multiple episodes, probably a double episode, searching again");

            _commandExecutor.PublishCommandAsync(new EpisodeSearchCommand
            {
                EpisodeIds = episodeIds.ToList()
            });
        }
    }
}
