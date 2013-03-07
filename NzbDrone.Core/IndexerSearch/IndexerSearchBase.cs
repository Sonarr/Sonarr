using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public abstract class IndexerSearchBase
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IEpisodeService _episodeService;
        private readonly DownloadProvider _downloadProvider;
        private readonly ISceneMappingService _sceneMappingService;
        private readonly IDownloadDirector DownloadDirector;

        protected readonly IIndexerService _indexerService;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected IndexerSearchBase(ISeriesRepository seriesRepository, IEpisodeService episodeService, DownloadProvider downloadProvider,
                             IIndexerService indexerService, ISceneMappingService sceneMappingService,
                             IDownloadDirector downloadDirector)
        {
            _seriesRepository = seriesRepository;
            _episodeService = episodeService;
            _downloadProvider = downloadProvider;
            _indexerService = indexerService;
            _sceneMappingService = sceneMappingService;
            DownloadDirector = downloadDirector;
        }

        protected IndexerSearchBase()
        {
        }

        public abstract List<EpisodeParseResult> PerformSearch(Series series, List<Episode> episodes, ProgressNotification notification);
        public abstract bool IsEpisodeMatch(Series series, dynamic options, EpisodeParseResult episodeParseResult);

        public virtual List<int> Search(Series series, dynamic options, ProgressNotification notification)
        {
            if (options == null)
                throw new ArgumentNullException(options);


            List<EpisodeParseResult> reports = PerformSearch(series, options, notification);

            logger.Debug("Finished searching all indexers. Total {0}", reports.Count);
            notification.CurrentMessage = "Processing search results";

            var result = ProcessReports(series, options, reports);

            if (!result.Grabbed.Any())
            {
                logger.Warn("Unable to find {0} in any of indexers.", options.Episode);

                notification.CurrentMessage = reports.Any() ? String.Format("Sorry, couldn't find {0}, that matches your preferences.", options.Episode)
                                                            : String.Format("Sorry, couldn't find {0} in any of indexers.", options.Episode);
            }

            return result.Grabbed;
        }

        public void ProcessReports(Series series, dynamic options, List<EpisodeParseResult> episodeParseResults)
        {

            var sortedResults = episodeParseResults.OrderByDescending(c => c.Quality)
                                                   .ThenBy(c => c.EpisodeNumbers.MinOrDefault())
                                                   .ThenBy(c => c.Age);

            foreach (var episodeParseResult in sortedResults)
            {
                try
                {

                    logger.Trace("Analyzing report " + episodeParseResult);
                    episodeParseResult.Series = _seriesRepository.GetByTitle(episodeParseResult.CleanTitle);

                    if (episodeParseResult.Series == null || episodeParseResult.Series.Id != series.Id)
                    {
                        episodeParseResult.Decision = new DownloadDecision("Invalid Series");
                        continue;
                    }

                    episodeParseResult.Episodes = _episodeService.GetEpisodesByParseResult(episodeParseResult);


                    if (!IsEpisodeMatch(series, options, episodeParseResult))
                    {
                        episodeParseResult.Decision = new DownloadDecision("Incorrect Episode/Season");
                    }

                    var downloadDecision = DownloadDirector.GetDownloadDecision(episodeParseResult);

                    if (downloadDecision.Approved)
                    {
                        DownloadReport(episodeParseResult);
                    }
                }
                catch (Exception e)
                {
                    logger.ErrorException("An error has occurred while processing parse result items from " + episodeParseResult, e);
                }
            }
        }

        public virtual Boolean DownloadReport(EpisodeParseResult episodeParseResult)
        {
            logger.Debug("Found '{0}'. Adding to download queue.", episodeParseResult);
            try
            {
                if (_downloadProvider.DownloadReport(episodeParseResult))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.ErrorException("Unable to add report to download queue." + episodeParseResult, e);
            }

            return false;
        }

        public virtual string GetSearchTitle(Series series, int seasonNumber = -1)
        {
            var seasonTitle = _sceneMappingService.GetSceneName(series.Id, seasonNumber);

            if (!String.IsNullOrWhiteSpace(seasonTitle))
                return seasonTitle;

            var title = _sceneMappingService.GetSceneName(series.Id);

            if (String.IsNullOrWhiteSpace(title))
            {
                title = series.Title;
                title = title.Replace("&", "and");
                title = Regex.Replace(title, @"[^\w\d\s\-]", "");
            }

            return title;
        }
    }
}
