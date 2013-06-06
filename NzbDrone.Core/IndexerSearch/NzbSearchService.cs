using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using System.Linq;

namespace NzbDrone.Core.IndexerSearch
{
    public interface ISearchForNzb
    {
        List<DownloadDecision> SearchSingle(int seriesId, int seasonNumber, int episodeNumber);
        List<DownloadDecision> SearchDaily(int seriesId, DateTime airDate);
        List<DownloadDecision> SearchSeason(int seriesId, int seasonNumber);
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

        public NzbSearchService(IIndexerService indexerService, IFetchFeedFromIndexers feedFetcher, ISceneMappingService sceneMapping, ISeriesService seriesService, IEpisodeService episodeService, IMakeDownloadDecision makeDownloadDecision, Logger logger)
        {
            _indexerService = indexerService;
            _feedFetcher = feedFetcher;
            _sceneMapping = sceneMapping;
            _seriesService = seriesService;
            _episodeService = episodeService;
            _makeDownloadDecision = makeDownloadDecision;
            _logger = logger;
        }

        public List<DownloadDecision> SearchSingle(int seriesId, int seasonNumber, int episodeNumber)
        {
            var searchSpec = Get<SingleEpisodeSearchCriteria>(seriesId, seasonNumber);

            if (_seriesService.GetSeries(seriesId).UseSceneNumbering)
            {
                var episode = _episodeService.GetEpisode(seriesId, seasonNumber, episodeNumber);
                searchSpec.EpisodeNumber = episode.SceneEpisodeNumber;
                searchSpec.SeasonNumber = episode.SceneSeasonNumber;
            }
            else
            {
                searchSpec.EpisodeNumber = episodeNumber;
                searchSpec.SeasonNumber = seasonNumber;
            }

            return Dispatch(indexer => _feedFetcher.Fetch(indexer, searchSpec), searchSpec);
        }

        public List<DownloadDecision> SearchDaily(int seriesId, DateTime airDate)
        {
            var searchSpec = Get<DailyEpisodeSearchCriteria>(seriesId);
            searchSpec.Airtime = airDate;

            return Dispatch(indexer => _feedFetcher.Fetch(indexer, searchSpec), searchSpec);
        }

        public List<DownloadDecision> SearchSeason(int seriesId, int seasonNumber)
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

        private TSpec Get<TSpec>(int seriesId, int seasonNumber = -1) where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            var tvdbId = _seriesService.GetSeries(seriesId).TvdbId;

            spec.SeriesId = seriesId;
            spec.SceneTitle = _sceneMapping.GetSceneName(tvdbId, seasonNumber);

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
                }));
            }

            Task.WaitAll(taskList.ToArray());

            _logger.Debug("Total of {0} reports were found for {1} in {2} indexers", reports.Count, criteriaBase, indexers.Count);

            return _makeDownloadDecision.GetSearchDecision(reports, criteriaBase).ToList();
        }
    }
}