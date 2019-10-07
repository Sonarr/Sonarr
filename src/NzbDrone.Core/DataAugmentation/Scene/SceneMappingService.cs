using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using System.Collections.Generic;
using NzbDrone.Core.Tv.Events;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingService
    {
        List<string> GetSceneNames(int tvdbId, List<int> seasonNumbers, List<int> sceneSeasonNumbers);
        int? FindTvdbId(string sceneTitle, string releaseTitle);
        List<SceneMapping> FindByTvdbId(int tvdbId);
        SceneMapping FindSceneMapping(string sceneTitle, string releaseTitle);
        int? GetSceneSeasonNumber(string seriesTitle, string releaseTitle);
        int? GetTvdbSeasonNumber(string seriesTitle, string releaseTitle);
        int? GetSceneSeasonNumber(int tvdbId, int seasonNumber);
    }

    public class SceneMappingService : ISceneMappingService,
                                       IHandle<SeriesRefreshStartingEvent>,
                                       IExecute<UpdateSceneMappingCommand>
    {
        private readonly ISceneMappingRepository _repository;
        private readonly IEnumerable<ISceneMappingProvider> _sceneMappingProviders;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;
        private readonly ICachedDictionary<List<SceneMapping>> _getTvdbIdCache;
        private readonly ICachedDictionary<List<SceneMapping>> _findByTvdbIdCache;

        public SceneMappingService(ISceneMappingRepository repository,
                                   ICacheManager cacheManager,
                                   IEnumerable<ISceneMappingProvider> sceneMappingProviders,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _repository = repository;
            _sceneMappingProviders = sceneMappingProviders;
            _eventAggregator = eventAggregator;
            _logger = logger;

            _getTvdbIdCache = cacheManager.GetCacheDictionary<List<SceneMapping>>(GetType(), "tvdb_id");
            _findByTvdbIdCache = cacheManager.GetCacheDictionary<List<SceneMapping>>(GetType(), "find_tvdb_id");
        }

        public List<string> GetSceneNames(int tvdbId, List<int> seasonNumbers, List<int> sceneSeasonNumbers)
        {
            var mappings = FindByTvdbId(tvdbId);

            if (mappings == null)
            {
                return new List<string>();
            }

            var names = mappings.Where(n => n.SeasonNumber.HasValue && seasonNumbers.Contains(n.SeasonNumber.Value) ||
                                            n.SceneSeasonNumber.HasValue && sceneSeasonNumbers.Contains(n.SceneSeasonNumber.Value) ||
                                            (n.SeasonNumber ?? -1) == -1 && (n.SceneSeasonNumber ?? -1) == -1)
                                .Select(n => n.SearchTerm).Distinct().ToList();

            return FilterNonEnglish(names);
        }

        public int? FindTvdbId(string seriesTitle)
        {
            return FindTvdbId(seriesTitle, null);
        }

        public int? FindTvdbId(string seriesTitle, string releaseTitle)
        {
            return FindSceneMapping(seriesTitle, releaseTitle)?.TvdbId;
        }

        public List<SceneMapping> FindByTvdbId(int tvdbId)
        {
            if (_findByTvdbIdCache.Count == 0)
            {
                RefreshCache();
            }

            var mappings = _findByTvdbIdCache.Find(tvdbId.ToString());

            if (mappings == null)
            {
                return new List<SceneMapping>();
            }

            return mappings;
        }

        public SceneMapping FindSceneMapping(string seriesTitle, string releaseTitle)
        {
            var mappings = FindMappings(seriesTitle, releaseTitle);

            if (mappings == null)
            {
                return null;
            }

            var distinctMappings = mappings.DistinctBy(v => v.TvdbId).ToList();

            if (distinctMappings.Count == 0)
            {
                return null;
            }

            if (distinctMappings.Count == 1)
            {
                var mapping = distinctMappings.First();
                _logger.Debug("Found scene mapping for: {0}. TVDB ID for mapping: {1}", seriesTitle, mapping.TvdbId);
                return distinctMappings.First();
            }

            throw new InvalidSceneMappingException(mappings, releaseTitle);
        }

        public int? GetSceneSeasonNumber(string seriesTitle, string releaseTitle)
        {
            return FindSceneMapping(seriesTitle, releaseTitle)?.SceneSeasonNumber;
        }

        public int? GetTvdbSeasonNumber(string seriesTitle, string releaseTitle)
        {
            return FindSceneMapping(seriesTitle, releaseTitle)?.SeasonNumber;
        }

        public int? GetSceneSeasonNumber(int tvdbId, int seasonNumber)
        {
            var mappings = FindByTvdbId(tvdbId);

            if (mappings == null)
            {
                return null;
            }

            var mapping = mappings.FirstOrDefault(e => e.SeasonNumber == seasonNumber && e.SceneSeasonNumber.HasValue);

            if (mapping == null)
            {
                return null;
            }

            return mapping.SceneSeasonNumber;
        }

        private void UpdateMappings()
        {
            _logger.Info("Updating Scene mappings");

            foreach (var sceneMappingProvider in _sceneMappingProviders)
            {
                try
                {
                    var mappings = sceneMappingProvider.GetSceneMappings();

                    if (mappings.Any())
                    {
                        _repository.Clear(sceneMappingProvider.GetType().Name);

                        mappings.RemoveAll(sceneMapping =>
                        {
                            if (sceneMapping.Title.IsNullOrWhiteSpace() ||
                                sceneMapping.SearchTerm.IsNullOrWhiteSpace())
                            {
                                _logger.Warn("Invalid scene mapping found for: {0}, skipping", sceneMapping.TvdbId);
                                return true;
                            }

                            return false;
                        });

                        foreach (var sceneMapping in mappings)
                        {
                            sceneMapping.ParseTerm = sceneMapping.Title.CleanSeriesTitle();
                            sceneMapping.Type = sceneMappingProvider.GetType().Name;
                        }

                        _repository.InsertMany(mappings.ToList());
                    }
                    else
                    {
                        _logger.Warn("Received empty list of mapping. will not update");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to Update Scene Mappings");
                }
            }

            RefreshCache();

            _eventAggregator.PublishEvent(new SceneMappingsUpdatedEvent());
        }

        private List<SceneMapping> FindMappings(string seriesTitle, string releaseTitle)
        {
            if (_getTvdbIdCache.Count == 0)
            {
                RefreshCache();
            }

            var candidates = _getTvdbIdCache.Find(seriesTitle.CleanSeriesTitle());

            if (candidates == null)
            {
                return null;
            }

            candidates = FilterSceneMappings(candidates, releaseTitle);

            if (candidates.Count <= 1)
            {
                return candidates;
            }

            var exactMatch = candidates.OrderByDescending(v => v.SeasonNumber)
                                       .Where(v => v.Title == seriesTitle)
                                       .ToList();

            if (exactMatch.Any())
            {
                return exactMatch;
            }

            var closestMatch = candidates.OrderBy(v => seriesTitle.LevenshteinDistance(v.Title, 10, 1, 10))
                                         .ThenByDescending(v => v.SeasonNumber)
                                         .First();


            return candidates.Where(v => v.Title == closestMatch.Title).ToList();
        }

        private void RefreshCache()
        {
            var mappings = _repository.All().ToList();

            _getTvdbIdCache.Update(mappings.GroupBy(v => v.ParseTerm).ToDictionary(v => v.Key, v => v.ToList()));
            _findByTvdbIdCache.Update(mappings.GroupBy(v => v.TvdbId).ToDictionary(v => v.Key.ToString(), v => v.ToList()));
        }

        private List<SceneMapping> FilterSceneMappings(List<SceneMapping> candidates, string releaseTitle)
        {
            var filteredCandidates = candidates.Where(v => v.FilterRegex.IsNotNullOrWhiteSpace()).ToList();
            var normalCandidates = candidates.Except(filteredCandidates).ToList();

            if (releaseTitle.IsNullOrWhiteSpace())
            {
                return normalCandidates;
            }

            filteredCandidates = filteredCandidates.Where(v => Regex.IsMatch(releaseTitle, v.FilterRegex)).ToList();

            if (filteredCandidates.Any())
            {
                return filteredCandidates;
            }

            return normalCandidates;
        }

        private List<string> FilterNonEnglish(List<string> titles)
        {
            return titles.Where(title => title.All(c => c <= 255)).ToList();
        }

        public void Handle(SeriesRefreshStartingEvent message)
        {
            if (message.ManualTrigger && _findByTvdbIdCache.IsExpired(TimeSpan.FromMinutes(1)))
            {
                UpdateMappings();
            }
        }

        public void Execute(UpdateSceneMappingCommand message)
        {
            UpdateMappings();
        }
    }
}
