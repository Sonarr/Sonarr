using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingService
    {
        string GetSceneName(int tvdbId, int seasonNumber = -1);
        Nullable<int> GetTvDbId(string cleanName);
    }

    public class SceneMappingService : ISceneMappingService,
        IHandleAsync<ApplicationStartedEvent>,
        IExecute<UpdateSceneMappingCommand>
    {

        private static readonly object mutex = new object();

        private readonly ISceneMappingRepository _repository;
        private readonly ISceneMappingProxy _sceneMappingProxy;
        private readonly Logger _logger;
        private readonly ICached<SceneMapping> _getSceneNameCache;
        private readonly ICached<SceneMapping> _gettvdbIdCache;

        public SceneMappingService(ISceneMappingRepository repository, ISceneMappingProxy sceneMappingProxy, ICacheManger cacheManger, Logger logger)
        {
            _repository = repository;
            _sceneMappingProxy = sceneMappingProxy;

            _getSceneNameCache = cacheManger.GetCache<SceneMapping>(GetType(), "scene_name");
            _gettvdbIdCache = cacheManger.GetCache<SceneMapping>(GetType(), "tvdb_id");
            _logger = logger;
        }

        public string GetSceneName(int tvdbId, int seasonNumber = -1)
        {
            lock (mutex)
            {
                var mapping = _getSceneNameCache.Find(tvdbId.ToString());

                if (mapping == null) return null;

                return mapping.SceneName;
            }
        }


        public Nullable<Int32> GetTvDbId(string cleanName)
        {
            lock (mutex)
            {
                var mapping = _gettvdbIdCache.Find(cleanName);

                if (mapping == null)
                    return null;

                return mapping.TvdbId;
            }
        }


        public void HandleAsync(ApplicationStartedEvent message)
        {
            if (!_repository.HasItems())
            {
                UpdateMappings();
            }
        }

        private void UpdateMappings()
        {
            try
            {
                var mappings = _sceneMappingProxy.Fetch();

                lock (mutex)
                {
                    if (mappings.Any())
                    {
                        _repository.Purge();
                        _repository.InsertMany(mappings);

                        _gettvdbIdCache.Clear();
                        _getSceneNameCache.Clear();

                        foreach (var sceneMapping in mappings)
                        {
                            _getSceneNameCache.Set(sceneMapping.TvdbId.ToString(), sceneMapping);
                            _gettvdbIdCache.Set(sceneMapping.CleanTitle, sceneMapping);
                        }
                    }
                    else
                    {
                        _logger.Warn("Received empty list of mapping. will not update.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Failed to Update Scene Mappings:", ex);
            }
        }

        public void Execute(UpdateSceneMappingCommand message)
        {
            UpdateMappings();
        }
    }
}
