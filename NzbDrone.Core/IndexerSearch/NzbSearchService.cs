using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using System.Linq;
using NzbDrone.Common.TPL;

namespace NzbDrone.Core.IndexerSearch
{
    public interface ISearchForNzb
    {
        List<DownloadDecision> EpisodeSearch(int episodeId);
        List<DownloadDecision> SeasonSearch(int seriesId, int seasonNumber);
    }

    public class NzbSearchService : ISearchForNzb
    {
        private readonly IIndexerService _indexerService;
        private readonly IFetchFeedFromIndexers _feedFetcher;
        private readonly ISceneMappingService _sceneMapping;
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IMakeDownloadDecision _makeDownloadDecision;
        private readonly Logger _logger;

        public NzbSearchService(IIndexerService indexerService,
                                IFetchFeedFromIndexers feedFetcher,
                                ISceneMappingService sceneMapping,
                                ISeriesService seriesService,
                                IEpisodeService episodeService,
                                IMakeDownloadDecision makeDownloadDecision,
                                Logger logger)
        {
            _indexerService = indexerService;
            _feedFetcher = feedFetcher;
            _sceneMapping = sceneMapping;
            _seriesService = seriesService;
            _episodeService = episodeService;
            _makeDownloadDecision = makeDownloadDecision;
            _logger = logger;
        }

        public List<DownloadDecision> EpisodeSearch(int episodeId)
        {
            var episode = _episodeService.GetEpisode(episodeId);
            var series = _seriesService.GetSeries(episode.SeriesId);

            if (series.SeriesType == SeriesTypes.Daily)
            {
                if (string.IsNullOrWhiteSpace(episode.AirDate))
                {
                    throw new InvalidOperationException("Daily episode is missing AirDate. Try to refresh series info.");
                }

                return SearchDaily(episode.SeriesId, episode.Series.TvRageId, DateTime.ParseExact(episode.AirDate, Episode.AIR_DATE_FORMAT, CultureInfo.InvariantCulture));
            }

            return SearchSingle(series, episode);
        }

        private List<DownloadDecision> SearchSingle(Series series, Episode episode)
        {
            var searchSpec = Get<SingleEpisodeSearchCriteria>(series.Id, series.TvRageId, episode.SeasonNumber);

            if (series.UseSceneNumbering)
            {
                if (episode.SceneSeasonNumber > 0 && episode.SceneEpisodeNumber > 0)
                {
                    searchSpec.EpisodeNumber = episode.SceneEpisodeNumber;
                    searchSpec.SeasonNumber = episode.SceneSeasonNumber;
                }

                else
                {
                    searchSpec.EpisodeNumber = episode.EpisodeNumber;
                    searchSpec.SeasonNumber = episode.SeasonNumber;
                }
            }
            else
            {
                searchSpec.EpisodeNumber = episode.EpisodeNumber;
                searchSpec.SeasonNumber = episode.SeasonNumber;
            }

            return Dispatch(indexer => _feedFetcher.Fetch(indexer, searchSpec), searchSpec);
        }

        private List<DownloadDecision> SearchDaily(int seriesId, int rageTvId, DateTime airDate)
        {
            var searchSpec = Get<DailyEpisodeSearchCriteria>(seriesId, rageTvId);
            searchSpec.Airtime = airDate;

            return Dispatch(indexer => _feedFetcher.Fetch(indexer, searchSpec), searchSpec);
        }

        public List<DownloadDecision> SeasonSearch(int seriesId, int seasonNumber)
        {
            var searchSpec = Get<SeasonSearchCriteria>(seriesId, seasonNumber);
            searchSpec.SeasonNumber = seasonNumber;

            return Dispatch(indexer => _feedFetcher.Fetch(indexer, searchSpec), searchSpec);
        }

        private List<DownloadDecision> PartialSeasonSearch(SeasonSearchCriteria search)
        {
            var episodesNumbers = _episodeService.GetEpisodesBySeason(search.SeriesId, search.SeasonNumber).Select(c => c.EpisodeNumber);
            var prefixes = episodesNumbers
                .Select(i => i / 10)
                .Distinct()
                .Select(prefix => new PartialSeasonSearchCriteria(search, prefix));

            var result = new List<DownloadDecision>();

            foreach (var partialSeasonSearchSpec in prefixes)
            {
                var spec = partialSeasonSearchSpec;
                result.AddRange(Dispatch(indexer => _feedFetcher.Fetch(indexer, spec), partialSeasonSearchSpec));
            }


            return result;
        }

        private TSpec Get<TSpec>(int seriesId, int rageTvId, int seasonNumber = -1) where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            var series = _seriesService.GetSeries(seriesId);

            spec.SeriesId = seriesId;
            spec.SeriesTvRageId = rageTvId;
            spec.SceneTitle = _sceneMapping.GetSceneName(series.TvdbId, seasonNumber);

            if (string.IsNullOrWhiteSpace(spec.SceneTitle))
            {
                spec.SceneTitle = series.Title;
            }

            return spec;
        }

        private List<DownloadDecision> Dispatch(Func<IIndexer, IEnumerable<ReportInfo>> searchAction, SearchCriteriaBase criteriaBase)
        {
            var indexers = _indexerService.GetAvailableIndexers().ToList();
            var reports = new List<ReportInfo>();


            var taskList = new List<Task>();
            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

            foreach (var indexer in indexers)
            {
                var indexerLocal = indexer;

                taskList.Add(taskFactory.StartNew(() =>
                {
                    try
                    {
                        var indexerReports = searchAction(indexerLocal);

                        lock (reports)
                        {
                            reports.AddRange(indexerReports);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.ErrorException("Error while searching for " + criteriaBase, e);
                    }
                }).LogExceptions());
            }

            Task.WaitAll(taskList.ToArray());

            _logger.Debug("Total of {0} reports were found for {1} in {2} indexers", reports.Count, criteriaBase, indexers.Count);

            return _makeDownloadDecision.GetSearchDecision(reports, criteriaBase).ToList();
        }
    }
}