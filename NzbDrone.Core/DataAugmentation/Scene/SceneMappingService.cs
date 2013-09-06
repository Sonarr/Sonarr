using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingService
    {
        string GetSceneName(int tvdbId);
        Nullable<int> GetTvDbId(string cleanName);
    }

    public class SceneMappingService : ISceneMappingService,
        IHandleAsync<ApplicationStartedEvent>,
        IExecute<UpdateSceneMappingCommand>
    {

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

        public string GetSceneName(int tvdbId)
        {
            var mapping = _getSceneNameCache.Find(tvdbId.ToString());

            if (mapping == null) return null;

            return mapping.SearchTerm;
        }

        public Nullable<Int32> GetTvDbId(string cleanName)
        {
            var mapping = _gettvdbIdCache.Find(cleanName.CleanSeriesTitle());

            if (mapping == null)
                return null;

            return mapping.TvdbId;
        }

        private void UpdateMappings()
        {
            _logger.Info("Updating Scene mapping");

            try
            {
                var mappings = _sceneMappingProxy.Fetch();

                if (mappings.Any())
                {
                    _repository.Purge();

                    foreach (var sceneMapping in mappings)
                    {
                        sceneMapping.ParseTerm = sceneMapping.ParseTerm.CleanSeriesTitle();
                    }

                    _repository.InsertMany(mappings);
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

            RefreshCache();
        }

        private void RefreshCache()
        {
            var mappings = _repository.All();

            _gettvdbIdCache.Clear();
            _getSceneNameCache.Clear();

            foreach (var sceneMapping in mappings)
            {
                _getSceneNameCache.Set(sceneMapping.TvdbId.ToString(), sceneMapping);
                _gettvdbIdCache.Set(sceneMapping.ParseTerm.CleanSeriesTitle(), sceneMapping);
            }
        }


        public void HandleAsync(ApplicationStartedEvent message)
        {
            UpdateMappings();
        }

        public void Execute(UpdateSceneMappingCommand message)
        {
            UpdateMappings();
        }
    }
}
