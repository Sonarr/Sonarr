using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.TPL;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public interface ISearchForReleases
    {
        List<DownloadDecision> EpisodeSearch(int episodeId, bool userInvokedSearch, bool interactiveSearch);
        List<DownloadDecision> EpisodeSearch(Episode episode, bool userInvokedSearch, bool interactiveSearch);
        List<DownloadDecision> SeasonSearch(int seriesId, int seasonNumber, bool missingOnly, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch);
        List<DownloadDecision> SeasonSearch(int seriesId, int seasonNumber, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch);
    }

    public class ReleaseSearchService : ISearchForReleases
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly ISceneMappingService _sceneMapping;
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IMakeDownloadDecision _makeDownloadDecision;
        private readonly Logger _logger;

        public ReleaseSearchService(IIndexerFactory indexerFactory,
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

                return SearchDaily(series, episode, false, userInvokedSearch, interactiveSearch);
            }

            if (series.SeriesType == SeriesTypes.Anime)
            {
                if (episode.SeasonNumber == 0 &&
                    episode.SceneAbsoluteEpisodeNumber == null &&
                    episode.AbsoluteEpisodeNumber == null)
                {
                    // Search for special episodes in season 0 that don't have absolute episode numbers
                    return SearchSpecial(series, new List<Episode> { episode }, false, userInvokedSearch, interactiveSearch);
                }

                return SearchAnime(series, episode, false, userInvokedSearch, interactiveSearch);
            }

            if (episode.SeasonNumber == 0)
            {
                // Search for special episodes in season 0
                return SearchSpecial(series, new List<Episode> { episode }, false, userInvokedSearch, interactiveSearch);
            }

            return SearchSingle(series, episode, false, userInvokedSearch, interactiveSearch);
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
                return SearchAnimeSeason(series, episodes, monitoredOnly, userInvokedSearch, interactiveSearch);
            }

            if (series.SeriesType == SeriesTypes.Daily)
            {
                return SearchDailySeason(series, episodes, monitoredOnly, userInvokedSearch, interactiveSearch);
            }

            var mappings = GetSceneSeasonMappings(series, episodes);

            var downloadDecisions = new List<DownloadDecision>();

            foreach (var mapping in mappings)
            {
                if (mapping.SeasonNumber == 0)
                {
                    // search for special episodes in season 0
                    downloadDecisions.AddRange(SearchSpecial(series, mapping.Episodes, monitoredOnly, userInvokedSearch, interactiveSearch));
                    continue;
                }

                if (mapping.Episodes.Count == 1)
                {
                    var searchSpec = Get<SingleEpisodeSearchCriteria>(series, mapping, monitoredOnly, userInvokedSearch, interactiveSearch);
                    searchSpec.SeasonNumber = mapping.SeasonNumber;
                    searchSpec.EpisodeNumber = mapping.EpisodeMapping.EpisodeNumber;

                    var decisions = Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                    downloadDecisions.AddRange(decisions);
                }
                else
                {
                    var searchSpec = Get<SeasonSearchCriteria>(series, mapping, monitoredOnly, userInvokedSearch, interactiveSearch);
                    searchSpec.SeasonNumber = mapping.SeasonNumber;

                    var decisions = Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                    downloadDecisions.AddRange(decisions);
                }
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private List<SceneSeasonMapping> GetSceneSeasonMappings(Series series, List<Episode> episodes)
        {
            var dict = new Dictionary<SceneSeasonMapping, SceneSeasonMapping>();

            var sceneMappings = _sceneMapping.FindByTvdbId(series.TvdbId);

            // Group the episode by SceneSeasonNumber/SeasonNumber, in 99% of cases this will result in 1 groupedEpisode
            var groupedEpisodes = episodes.ToLookup(v => ((v.SceneSeasonNumber ?? v.SeasonNumber) * 100000) + v.SeasonNumber);

            foreach (var groupedEpisode in groupedEpisodes)
            {
                var episodeMappings = GetSceneEpisodeMappings(series, groupedEpisode.First(), sceneMappings);

                foreach (var episodeMapping in episodeMappings)
                {
                    var seasonMapping = new SceneSeasonMapping
                    {
                        Episodes = groupedEpisode.ToList(),
                        EpisodeMapping = episodeMapping,
                        SceneTitles = episodeMapping.SceneTitles,
                        SearchMode = episodeMapping.SearchMode,
                        SeasonNumber = episodeMapping.SeasonNumber
                    };

                    if (dict.TryGetValue(seasonMapping, out var existing))
                    {
                        existing.Episodes.AddRange(seasonMapping.Episodes);
                        existing.SceneTitles.AddRange(seasonMapping.SceneTitles);
                    }
                    else
                    {
                        dict[seasonMapping] = seasonMapping;
                    }
                }
            }

            foreach (var item in dict)
            {
                item.Value.Episodes = item.Value.Episodes.Distinct().ToList();
                item.Value.SceneTitles = item.Value.SceneTitles.Distinct().ToList();
            }

            return dict.Values.ToList();
        }

        private List<SceneEpisodeMapping> GetSceneEpisodeMappings(Series series, Episode episode)
        {
            var dict = new Dictionary<SceneEpisodeMapping, SceneEpisodeMapping>();

            var sceneMappings = _sceneMapping.FindByTvdbId(series.TvdbId);

            var episodeMappings = GetSceneEpisodeMappings(series, episode, sceneMappings);

            foreach (var episodeMapping in episodeMappings)
            {
                if (dict.TryGetValue(episodeMapping, out var existing))
                {
                    existing.SceneTitles.AddRange(episodeMapping.SceneTitles);
                }
                else
                {
                    dict[episodeMapping] = episodeMapping;
                }
            }

            foreach (var item in dict)
            {
                item.Value.SceneTitles = item.Value.SceneTitles.Distinct().ToList();
            }

            return dict.Values.ToList();
        }

        private IEnumerable<SceneEpisodeMapping> GetSceneEpisodeMappings(Series series, Episode episode, List<SceneMapping> sceneMappings)
        {
            var includeGlobal = true;

            foreach (var sceneMapping in sceneMappings)
            {
                // There are two kinds of mappings:
                // - Mapped on Release Season Number with sceneMapping.SceneSeasonNumber specified and optionally sceneMapping.SeasonNumber. This translates via episode.SceneSeasonNumber/SeasonNumber to specific episodes.
                // - Mapped on Episode Season Number with optionally sceneMapping.SeasonNumber. This translates from episode.SceneSeasonNumber/SeasonNumber to specific releases. (Filter by episode.SeasonNumber or globally)

                var ignoreSceneNumbering = sceneMapping.SceneOrigin == "tvdb" || sceneMapping.SceneOrigin == "unknown:tvdb";
                var mappingSceneSeasonNumber = sceneMapping.SceneSeasonNumber.NonNegative();
                var mappingSeasonNumber = sceneMapping.SeasonNumber.NonNegative();

                // Select scene or tvdb on the episode
                var mappedSeasonNumber = ignoreSceneNumbering ? episode.SeasonNumber : (episode.SceneSeasonNumber ?? episode.SeasonNumber);
                var releaseSeasonNumber = sceneMapping.SceneSeasonNumber.NonNegative() ?? mappedSeasonNumber;

                if (mappingSceneSeasonNumber.HasValue)
                {
                    // Apply the alternative mapping (release to scene/tvdb)
                    var mappedAltSeasonNumber = sceneMapping.SeasonNumber.NonNegative() ?? sceneMapping.SceneSeasonNumber.NonNegative() ?? mappedSeasonNumber;

                    // Check if the mapping applies to the current season
                    if (mappedAltSeasonNumber != mappedSeasonNumber)
                    {
                        continue;
                    }
                }
                else
                {
                    // Check if the mapping applies to the current season
                    if (mappingSeasonNumber.HasValue && mappingSeasonNumber.Value != episode.SeasonNumber)
                    {
                        continue;
                    }
                }

                if (sceneMapping.ParseTerm == series.CleanTitle && sceneMapping.FilterRegex.IsNotNullOrWhiteSpace())
                {
                    // Disable the implied mapping if we have an explicit mapping by the same name
                    includeGlobal = false;
                }

                // By default we do a alt title search in case indexers don't have the release properly indexed.  Services can override this behavior.
                var searchMode = sceneMapping.SearchMode ?? ((mappingSceneSeasonNumber.HasValue && series.CleanTitle != sceneMapping.SearchTerm.CleanSeriesTitle()) ? SearchMode.SearchTitle : SearchMode.Default);

                if (ignoreSceneNumbering)
                {
                    yield return new SceneEpisodeMapping
                    {
                        Episode = episode,
                        SearchMode = searchMode,
                        SceneTitles = new List<string> { sceneMapping.SearchTerm },
                        SeasonNumber = releaseSeasonNumber,
                        EpisodeNumber = episode.EpisodeNumber,
                        AbsoluteEpisodeNumber = episode.AbsoluteEpisodeNumber
                    };
                }
                else
                {
                    yield return new SceneEpisodeMapping
                    {
                        Episode = episode,
                        SearchMode = searchMode,
                        SceneTitles = new List<string> { sceneMapping.SearchTerm },
                        SeasonNumber = releaseSeasonNumber,
                        EpisodeNumber = episode.SceneEpisodeNumber ?? episode.EpisodeNumber,
                        AbsoluteEpisodeNumber = episode.SceneAbsoluteEpisodeNumber ?? episode.AbsoluteEpisodeNumber
                    };
                }
            }

            if (includeGlobal)
            {
                yield return new SceneEpisodeMapping
                {
                    Episode = episode,
                    SearchMode = SearchMode.Default,
                    SceneTitles = new List<string> { series.Title },
                    SeasonNumber = episode.SceneSeasonNumber ?? episode.SeasonNumber,
                    EpisodeNumber = episode.SceneEpisodeNumber ?? episode.EpisodeNumber,
                    AbsoluteEpisodeNumber = episode.SceneSeasonNumber ?? episode.AbsoluteEpisodeNumber
                };
            }
        }

        private List<DownloadDecision> SearchSingle(Series series, Episode episode, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var mappings = GetSceneEpisodeMappings(series, episode);

            var downloadDecisions = new List<DownloadDecision>();

            foreach (var mapping in mappings)
            {
                var searchSpec = Get<SingleEpisodeSearchCriteria>(series, mapping, monitoredOnly, userInvokedSearch, interactiveSearch);
                searchSpec.SeasonNumber = mapping.SeasonNumber;
                searchSpec.EpisodeNumber = mapping.EpisodeNumber;

                var decisions = Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
                downloadDecisions.AddRange(decisions);
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private List<DownloadDecision> SearchDaily(Series series, Episode episode, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var airDate = DateTime.ParseExact(episode.AirDate, Episode.AIR_DATE_FORMAT, CultureInfo.InvariantCulture);
            var searchSpec = Get<DailyEpisodeSearchCriteria>(series, new List<Episode> { episode }, monitoredOnly, userInvokedSearch, interactiveSearch);
            searchSpec.AirDate = airDate;

            return Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
        }

        private List<DownloadDecision> SearchAnime(Series series, Episode episode, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch, bool isSeasonSearch = false)
        {
            var searchSpec = Get<AnimeEpisodeSearchCriteria>(series, new List<Episode> { episode }, monitoredOnly, userInvokedSearch, interactiveSearch);

            searchSpec.IsSeasonSearch = isSeasonSearch;

            searchSpec.SeasonNumber = episode.SceneSeasonNumber ?? episode.SeasonNumber;
            searchSpec.EpisodeNumber = episode.SceneEpisodeNumber ?? episode.EpisodeNumber;
            searchSpec.AbsoluteEpisodeNumber = episode.SceneAbsoluteEpisodeNumber ?? episode.AbsoluteEpisodeNumber ?? 0;

            return Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
        }

        private List<DownloadDecision> SearchSpecial(Series series, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();

            var searchSpec = Get<SpecialEpisodeSearchCriteria>(series, episodes, monitoredOnly, userInvokedSearch, interactiveSearch);

            // build list of queries for each episode in the form: "<series> <episode-title>"
            searchSpec.EpisodeQueryTitles = episodes.Where(e => !string.IsNullOrWhiteSpace(e.Title))
                                                    .SelectMany(e => searchSpec.CleanSceneTitles.Select(title => title + " " + SearchCriteriaBase.GetCleanSceneTitle(e.Title)))
                                                    .ToArray();

            downloadDecisions.AddRange(Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec));

            // Search for each episode by season/episode number as well
            foreach (var episode in episodes)
            {
                // Episode needs to be monitored if it's not an interactive search
                if (!interactiveSearch && monitoredOnly && !episode.Monitored)
                {
                    continue;
                }

                downloadDecisions.AddRange(SearchSingle(series, episode, monitoredOnly, userInvokedSearch, interactiveSearch));
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private List<DownloadDecision> SearchAnimeSeason(Series series, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();

            // Episode needs to be monitored if it's not an interactive search
            // and Ensure episode has an airdate and has already aired
            var episodesToSearch = episodes
                .Where(ep => interactiveSearch || !monitoredOnly || ep.Monitored)
                .Where(ep => ep.AirDateUtc.HasValue && ep.AirDateUtc.Value.Before(DateTime.UtcNow))
                .ToList();

            foreach (var episode in episodesToSearch)
            {
                downloadDecisions.AddRange(SearchAnime(series, episode, monitoredOnly, userInvokedSearch, interactiveSearch, true));
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private List<DownloadDecision> SearchDailySeason(Series series, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();

            // Episode needs to be monitored if it's not an interactive search
            // and Ensure episode has an airdate
            var episodesToSearch = episodes
                .Where(ep => interactiveSearch || !monitoredOnly || ep.Monitored)
                .Where(ep => ep.AirDate.IsNotNullOrWhiteSpace())
                .ToList();

            foreach (var yearGroup in episodesToSearch.GroupBy(v => DateTime.ParseExact(v.AirDate, Episode.AIR_DATE_FORMAT, CultureInfo.InvariantCulture).Year))
            {
                var yearEpisodes = yearGroup.ToList();

                if (yearEpisodes.Count > 1)
                {
                    var searchSpec = Get<DailySeasonSearchCriteria>(series, yearEpisodes, monitoredOnly, userInvokedSearch, interactiveSearch);
                    searchSpec.Year = yearGroup.Key;

                    downloadDecisions.AddRange(Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec));
                }
                else
                {
                    downloadDecisions.AddRange(SearchDaily(series, yearEpisodes.First(), monitoredOnly, userInvokedSearch, interactiveSearch));
                }
            }

            return DeDupeDecisions(downloadDecisions);
        }

        private TSpec Get<TSpec>(Series series, List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            spec.Series = series;
            spec.SceneTitles = _sceneMapping.GetSceneNames(series.TvdbId,
                                                           episodes.Select(e => e.SeasonNumber).Distinct().ToList(),
                                                           episodes.Select(e => e.SceneSeasonNumber ?? e.SeasonNumber).Distinct().ToList());

            spec.Episodes = episodes;
            spec.MonitoredEpisodesOnly = monitoredOnly;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            if (!spec.SceneTitles.Contains(series.Title))
            {
                spec.SceneTitles.Add(series.Title);
            }

            return spec;
        }

        private TSpec Get<TSpec>(Series series, SceneEpisodeMapping mapping, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            spec.Series = series;
            spec.SceneTitles = mapping.SceneTitles;
            spec.SearchMode = mapping.SearchMode;

            spec.Episodes = new List<Episode> { mapping.Episode };
            spec.MonitoredEpisodesOnly = monitoredOnly;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            return spec;
        }

        private TSpec Get<TSpec>(Series series, SceneSeasonMapping mapping, bool monitoredOnly, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            spec.Series = series;
            spec.SceneTitles = mapping.SceneTitles;
            spec.SearchMode = mapping.SearchMode;

            spec.Episodes = mapping.Episodes;
            spec.MonitoredEpisodesOnly = monitoredOnly;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            return spec;
        }

        private List<DownloadDecision> Dispatch(Func<IIndexer, IEnumerable<ReleaseInfo>> searchAction, SearchCriteriaBase criteriaBase)
        {
            var indexers = criteriaBase.InteractiveSearch ?
                _indexerFactory.InteractiveSearchEnabled() :
                _indexerFactory.AutomaticSearchEnabled();

            // Filter indexers to untagged indexers and indexers with intersecting tags
            indexers = indexers.Where(i => i.Definition.Tags.Empty() || i.Definition.Tags.Intersect(criteriaBase.Series.Tags).Any()).ToList();

            var reports = new List<ReleaseInfo>();

            _logger.ProgressInfo("Searching indexers for {0}. {1} active indexers", criteriaBase, indexers.Count);

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

        private List<DownloadDecision> DeDupeDecisions(List<DownloadDecision> decisions)
        {
            // De-dupe reports by guid so duplicate results aren't returned. Pick the one with the least rejections.

            return decisions.GroupBy(d => d.RemoteEpisode.Release.Guid).Select(d => d.OrderBy(v => v.Rejections.Count()).First()).ToList();
        }
    }
}
