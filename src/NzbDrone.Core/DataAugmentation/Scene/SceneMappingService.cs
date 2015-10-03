using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using System.Collections.Generic;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingService
    {
        List<string> GetSceneNames(int tvdbId, IEnumerable<int> seasonNumbers);
        int? FindTvdbId(string title);
        List<SceneMapping> FindByTvdbId(int tvdbId);
        int? GetSeasonNumber(string title);
    }

    public class SceneMappingService : ISceneMappingService,
                                       IHandleAsync<ApplicationStartedEvent>,
                                       IHandle<SeriesRefreshStartingEvent>,
                                       IExecute<UpdateSceneMappingCommand>
    {
        private readonly ISceneMappingRepository _repository;
        private readonly IEnumerable<ISceneMappingProvider> _sceneMappingProviders;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;
        private readonly ICached<List<SceneMapping>> _getTvdbIdCache;
        private readonly ICached<List<SceneMapping>> _findByTvdbIdCache;

        public SceneMappingService(ISceneMappingRepository repository,
                                   ICacheManager cacheManager,
                                   IEnumerable<ISceneMappingProvider> sceneMappingProviders,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _repository = repository;
            _sceneMappingProviders = sceneMappingProviders;
            _eventAggregator = eventAggregator;

            _getTvdbIdCache = cacheManager.GetCache<List<SceneMapping>>(GetType(), "tvdb_id");
            _findByTvdbIdCache = cacheManager.GetCache<List<SceneMapping>>(GetType(), "find_tvdb_id");
            _logger = logger;
        }

        public List<string> GetSceneNames(int tvdbId, IEnumerable<int> seasonNumbers)
        {
            var names = _findByTvdbIdCache.Find(tvdbId.ToString());

            if (names == null)
            {
                return new List<string>();
            }

            return FilterNonEnglish(names.Where(s => seasonNumbers.Contains(s.SeasonNumber) ||
                                                     s.SeasonNumber == -1)
                                         .Select(m => m.SearchTerm).Distinct().ToList());
        }

        public int? FindTvdbId(string title)
        {
            var mapping = FindMapping(title);

            if (mapping == null)
                return null;

            return mapping.TvdbId;
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

        public int? GetSeasonNumber(string title)
        {
            var mapping = FindMapping(title);

            if (mapping == null)
                return null;

            return mapping.SeasonNumber;
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
                        _logger.Warn("Received empty list of mapping. will not update.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Failed to Update Scene Mappings:", ex);
                }
            }
            
            RefreshCache();
            _eventAggregator.PublishEvent(new SceneMappingsUpdatedEvent());
        }

        private SceneMapping FindMapping(string title)
        {
            if (_getTvdbIdCache.Count == 0)
            {
                RefreshCache();
            }

            var candidates = _getTvdbIdCache.Find(title.CleanSeriesTitle());

            if (candidates == null)
            {
                return null;
            }

            if (candidates.Count == 1)
            {
                return candidates.First();
            }

            var exactMatch = candidates.OrderByDescending(v => v.SeasonNumber)
                                       .FirstOrDefault(v => v.Title == title);

            if (exactMatch != null)
            {
                return exactMatch;
            }

            var closestMatch = candidates.OrderBy(v => title.LevenshteinDistance(v.Title, 10, 1, 10))
                                         .ThenByDescending(v => v.SeasonNumber)
                                         .First();

            return closestMatch;
        }

        private void RefreshCache()
        {
            var mappings = _repository.All().ToList();

            _getTvdbIdCache.Clear();
            _findByTvdbIdCache.Clear();

            foreach (var sceneMapping in mappings.GroupBy(v => v.ParseTerm))
            {
                _getTvdbIdCache.Set(sceneMapping.Key, sceneMapping.ToList());
            }

            foreach (var sceneMapping in mappings.GroupBy(x => x.TvdbId))
            {
                _findByTvdbIdCache.Set(sceneMapping.Key.ToString(), sceneMapping.ToList());
            }
        }

        private List<string> FilterNonEnglish(List<string> titles)
        {
            return titles.Where(title => title.All(c => c <= 255)).ToList();
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            UpdateMappings();
        }

        public void Handle(SeriesRefreshStartingEvent message)
        {
            UpdateMappings();
        }

        public void Execute(UpdateSceneMappingCommand message)
        {
            UpdateMappings();
        }
    }
}
