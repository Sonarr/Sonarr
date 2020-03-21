using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using System.Linq;
using NzbDrone.Common.TPL;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.IndexerSearch
{
    public interface ISearchForNzb
    {
        List<DownloadDecision> EpisodeSearch(int episodeId, bool userInvokedSearch, bool interactiveSearch);
        List<DownloadDecision> EpisodeSearch(Episode episode, bool userInvokedSearch, bool interactiveSearch);
        List<DownloadDecision> SeasonSearch(int seriesId, int seasonNumber, bool missingOnly, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch);
        List<DownloadDecision> SeasonSearch(int seriesId, int seasonNumber, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch);
    }

    public class NzbSearchService : ISearchForNzb
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly ISceneMappingService _sceneMapping;
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IMakeDownloadDecision _makeDownloadDecision;
        private readonly Logger _logger;

        public NzbSearchService(IIndexerFactory indexerFactory,
                                ISceneMappingService sceneMapping,
                                ISeriesService seriesService,
                                IEpisodeService episodeService,
                                IMakeDownloadDecision makeDownloadDecision,
                                Logger logger)
        {
            _indexerFactory = indexerFactory;
            _sceneMapping = sceneMapping;
            _seriesService = seriesService;
            _episodeService = episodeService;
            _makeDownloadDecision = makeDownloadDecision;
            _logger = logger;
        }

        public List<DownloadDecision> EpisodeSearch(int episodeId, bool userInvokedSearch, bool interactiveSearch)
        {
            var episode = _episodeService.GetEpisode(episodeId);

            return EpisodeSearch(episode, userInvokedSearch, interactiveSearch);
        }

        public List<DownloadDecision> EpisodeSearch(Episode episode, bool userInvokedSearch, bool interactiveSearch)
        {
            var series = _seriesService.GetSeries(episode.SeriesId);

            if (series.SeriesType == SeriesTypes.Daily)
            {
                if (string.IsNullOrWhiteSpace(episode.AirDate))
                {
                    _logger.Error("Daily episode is missing an air date. Try refreshing the series info.");
                    throw new SearchFailedException("Air date is missing");
                }

                return SearchDaily(series, episode, userInvokedSearch, interactiveSearch);
            }
            if (series.SeriesType == SeriesTypes.Anime)
            {
                return SearchAnime(series, episode, userInvokedSearch, interactiveSearch);
            }

            if (episode.SeasonNumber == 0)
            {
                // search for special episodes in season 0
                return SearchSpecial(series, new List<Episode> { episode }, userInvokedSearch, interactiveSearch);
            }

            return SearchSingle(series, episode, userInvokedSearch, interactiveSearch);
        }

        public List<DownloadDecision> SeasonSearch(int seriesId, int seasonNumber, bool missingOnly, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var episodes = _episodeService.GetEpisodesBySeason(seriesId, seasonNumber);

            if (missingOnly)
            {
                episodes = episodes.Where(e => !e.HasFile).ToList();
            }

            return SeasonSearch(seriesId, seasonNumber, episodes, monitoredOnly, userInvokedSearch, interactiveSearch);
        }
        public List<DownloadDecision> SeasonSearch(int seriesId, int seasonNumber, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var series = _seriesService.GetSeries(seriesId);

            if (series.SeriesType == SeriesTypes.Anime)
            {
                return SearchAnimeSeason(series, episodes, userInvokedSearch, interactiveSearch);
            }

            if (series.SeriesType == SeriesTypes.Daily)
            {
                return SearchDailySeason(series, episodes, userInvokedSearch, interactiveSearch);
            }

            if (seasonNumber == 0)
            {
                // search for special episodes in season 0
                return SearchSpecial(series, episodes, userInvokedSearch, interactiveSearch);
            }

            var downloadDecisions = new List<DownloadDecision>();

            if (series.UseSceneNumbering)
            {
                var sceneSeasonGroups = episodes.GroupBy(v =>
                {
                    if (v.SceneSeasonNumber.HasValue && v.SceneEpisodeNumber.HasValue)
                    {
                        return v.SceneSeasonNumber.Value;
                    }
                    return v.SeasonNumber;
                }).Distinct();

                foreach (var sceneSeasonEpisodes in sceneSeasonGroups)
                {
                    if (sceneSeasonEpisodes.Count() == 1)
                    {
                        var episode = sceneSeasonEpisodes.First();
                        var searchSpec = Get<SingleEpisodeSearchCriteria>(series, sceneSeasonEpisodes.ToList(), userInvokedSearch, interactiveSearch);

                        searchSpec.SeasonNumber = sceneSeasonEpisodes.Key;
                        searchSpec.MonitoredEpisodesOnly = monitoredOnly;

                        if (episode.SceneSeasonNumber.HasValue && episode.SceneEpisodeNumber.HasValue)
                        {
                            searchSpec.EpisodeNumber = episode.SceneEpisodeNumber.Value;
                        }
                        else
                        {
                            searchSpec.EpisodeNumber = episode.EpisodeNumber;
                        }

                        var decisions = Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                        downloadDecisions.AddRange(decisions);
                    }
                    else
                    {
                        var searchSpec = Get<SeasonSearchCriteria>(series, sceneSeasonEpisodes.ToList(), userInvokedSearch, interactiveSearch);
                        searchSpec.SeasonNumber = sceneSeasonEpisodes.Key;
                        searchSpec.MonitoredEpisodesOnly = monitoredOnly;

                        var decisions = Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                        downloadDecisions.AddRange(decisions);
                    }
                }
            }
            else
            {
                var searchSpec = Get<SeasonSearchCriteria>(series, episodes, userInvokedSearch, interactiveSearch);
                searchSpec.SeasonNumber = seasonNumber;
                searchSpec.MonitoredEpisodesOnly = monitoredOnly;

                var decisions = Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                downloadDecisions.AddRange(decisions);
            }

            return downloadDecisions;
        }

        private List<DownloadDecision> SearchSingle(Series series, Episode episode, bool userInvokedSearch, bool interactiveSearch)
        {
            var searchSpec = Get<SingleEpisodeSearchCriteria>(series, new List<Episode> { episode }, userInvokedSearch, interactiveSearch);

            if (series.UseSceneNumbering && episode.SceneSeasonNumber.HasValue && episode.SceneEpisodeNumber.HasValue)
            {
                searchSpec.EpisodeNumber = episode.SceneEpisodeNumber.Value;
                searchSpec.SeasonNumber = episode.SceneSeasonNumber.Value;
            }
            else
            {
                searchSpec.EpisodeNumber = episode.EpisodeNumber;
                searchSpec.SeasonNumber = episode.SeasonNumber;
            }

            return Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
        }

        private List<DownloadDecision> SearchDaily(Series series, Episode episode, bool userInvokedSearch, bool interactiveSearch)
        {
            var airDate = DateTime.ParseExact(episode.AirDate, Episode.AIR_DATE_FORMAT, CultureInfo.InvariantCulture);
            var searchSpec = Get<DailyEpisodeSearchCriteria>(series, new List<Episode> { episode }, userInvokedSearch, interactiveSearch);
            searchSpec.AirDate = airDate;

            return Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
        }

        private List<DownloadDecision> SearchAnime(Series series, Episode episode, bool userInvokedSearch, bool interactiveSearch, bool isSeasonSearch = false)
        {
            var searchSpec = Get<AnimeEpisodeSearchCriteria>(series, new List<Episode> { episode }, userInvokedSearch, interactiveSearch);

            searchSpec.IsSeasonSearch = isSeasonSearch;

            if (episode.SceneAbsoluteEpisodeNumber.HasValue)
            {
                searchSpec.AbsoluteEpisodeNumber = episode.SceneAbsoluteEpisodeNumber.Value;
            }
            else if (episode.AbsoluteEpisodeNumber.HasValue)
            {
                searchSpec.AbsoluteEpisodeNumber = episode.AbsoluteEpisodeNumber.Value;
            }
            else
            {
                _logger.Error($"Can not search for {series.Title} - S{episode.SeasonNumber:00}E{episode.EpisodeNumber:00} it does not have an absolute episode number");
                throw new SearchFailedException("Absolute episode number is missing");
            }

            return Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
        }

        private List<DownloadDecision> SearchSpecial(Series series, List<Episode> episodes, bool userInvokedSearch, bool interactiveSearch)
        {
            var searchSpec = Get<SpecialEpisodeSearchCriteria>(series, episodes, userInvokedSearch, interactiveSearch);
            // build list of queries for each episode in the form: "<series> <episode-title>"
            searchSpec.EpisodeQueryTitles = episodes.Where(e => !string.IsNullOrWhiteSpace(e.Title))
                                                    .SelectMany(e => searchSpec.QueryTitles.Select(title => title + " " + SearchCriteriaBase.GetQueryTitle(e.Title)))
                                                    .ToArray();

            return Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
        }

        private List<DownloadDecision> SearchAnimeSeason(Series series, List<Episode> episodes, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();

            // Only search for aired episodes when performing a season anime search
            foreach (var episode in episodes.Where(e => e.Monitored && e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(DateTime.UtcNow)))
            {
                downloadDecisions.AddRange(SearchAnime(series, episode, userInvokedSearch, interactiveSearch, true));
            }

            return downloadDecisions;
        }

        private List<DownloadDecision> SearchDailySeason(Series series, List<Episode> episodes, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();
            foreach (var yearGroup in episodes.Where(v => v.Monitored && v.AirDate.IsNotNullOrWhiteSpace())
                                              .GroupBy(v => DateTime.ParseExact(v.AirDate, Episode.AIR_DATE_FORMAT, CultureInfo.InvariantCulture).Year))
            {
                var yearEpisodes = yearGroup.ToList();

                if (yearEpisodes.Count > 1)
                {
                    var searchSpec = Get<DailySeasonSearchCriteria>(series, yearEpisodes, userInvokedSearch, interactiveSearch);
                    searchSpec.Year = yearGroup.Key;

                    downloadDecisions.AddRange(Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec));
                }
                else
                {
                    downloadDecisions.AddRange(SearchDaily(series, yearEpisodes.First(), userInvokedSearch, interactiveSearch));
                }
            }

            return downloadDecisions;
        }

        private TSpec Get<TSpec>(Series series, List<Episode> episodes, bool userInvokedSearch, bool interactiveSearch) where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            spec.Series = series;
            spec.SceneTitles = _sceneMapping.GetSceneNames(series.TvdbId,
                                                           episodes.Select(e => e.SeasonNumber).Distinct().ToList(),
                                                           episodes.Select(e => e.SceneSeasonNumber ?? e.SeasonNumber).Distinct().ToList());

            if (!spec.SceneTitles.Contains(series.Title))
            {
                spec.SceneTitles.Add(series.Title);
            }

            spec.Episodes = episodes;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            return spec;
        }

        private List<DownloadDecision> Dispatch(Func<IIndexer, IEnumerable<ReleaseInfo>> searchAction, SearchCriteriaBase criteriaBase)
        {
            var indexers = criteriaBase.InteractiveSearch ?
                _indexerFactory.InteractiveSearchEnabled() :
                _indexerFactory.AutomaticSearchEnabled();

            var reports = new List<ReleaseInfo>();

            _logger.ProgressInfo("Searching {0} indexers for {1}", indexers.Count, criteriaBase);

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
                        _logger.Error(e, "Error while searching for {0}", criteriaBase);
                    }
                }).LogExceptions());
            }

            Task.WaitAll(taskList.ToArray());

            _logger.Debug("Total of {0} reports were found for {1} from {2} indexers", reports.Count, criteriaBase, indexers.Count);

            // Update the last search time for all episodes if at least 1 indexer was searched.
            if (indexers.Any())
            {
                var lastSearchTime = DateTime.UtcNow;
                _logger.Debug("Setting last search time to: {0}", lastSearchTime);

                criteriaBase.Episodes.ForEach(e => e.LastSearchTime = lastSearchTime);
                _episodeService.UpdateEpisodes(criteriaBase.Episodes);
            }

            return _makeDownloadDecision.GetSearchDecision(reports, criteriaBase).ToList();
        }
    }
}
