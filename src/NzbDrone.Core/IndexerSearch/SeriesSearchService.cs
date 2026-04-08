using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeriesSearchService : IExecute<SeriesSearchCommand>
    {
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public SeriesSearchService(ISeriesService seriesService,
                                   IEpisodeService episodeService,
                                   ISearchForReleases releaseSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions,
                                   Logger logger)
        {
            _seriesService = seriesService;
            _episodeService = episodeService;
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(SeriesSearchCommand message)
        {
            var series = _seriesService.GetSeries(message.SeriesId);
            var downloadedCount = 0;
            var userInvokedSearch = message.Trigger == CommandTrigger.Manual;
            var profile = series.QualityProfile.Value;

            if (series.Seasons.None(s => s.Monitored))
            {
                _logger.Debug("No seasons of {SeriesTitle} are monitored, searching for all monitored episodes", series.Title);

                var episodes = _episodeService.GetEpisodeBySeries(series.Id)
                    .Where(e => e.Monitored &&
                                !e.HasFile &&
                                e.AirDateUtc.HasValue &&
                                e.AirDateUtc.Value.Before(DateTime.UtcNow))
                    .ToList();

                foreach (var episode in episodes)
                {
                    var decisions = _releaseSearchService.EpisodeSearch(episode, userInvokedSearch, false).GetAwaiter().GetResult();
                    var processDecisions = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();
                    downloadedCount += processDecisions.Grabbed.Count;
                }
            }
            else
            {
                foreach (var season in series.Seasons.OrderBy(s => s.SeasonNumber))
                {
                    if (!season.Monitored)
                    {
                        _logger.Debug("Season {SeasonNumber} of {SeriesTitle} is not monitored, skipping search", season.SeasonNumber, series.Title);
                        continue;
                    }

                    var decisions = _releaseSearchService.SeasonSearch(message.SeriesId, season.SeasonNumber, !profile.UpgradeAllowed, true, userInvokedSearch, false).GetAwaiter().GetResult();
                    var processDecisions = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();
                    downloadedCount += processDecisions.Grabbed.Count;
                }
            }

            _logger.ProgressInfo("Series search completed. {DownloadedCount} reports downloaded.", downloadedCount);
        }
    }
}
