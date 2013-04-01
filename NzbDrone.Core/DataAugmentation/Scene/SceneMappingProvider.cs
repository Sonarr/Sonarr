using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingService
    {
        void UpdateMappings();
        string GetSceneName(int tvdbId, int seasonNumber = -1);
        Nullable<int> GetTvDbId(string cleanName);
        string GetCleanName(int tvdbId);
    }

    public class SceneMappingService : IInitializable, ISceneMappingService
    {
        private readonly ISceneMappingRepository _repository;
        private readonly ISceneMappingProxy _sceneMappingProxy;
        private readonly Logger _logger;

        public SceneMappingService(ISceneMappingRepository repository, ISceneMappingProxy sceneMappingProxy, Logger logger)
        {
            _repository = repository;
            _sceneMappingProxy = sceneMappingProxy;
            _logger = logger;
        }

        public void UpdateMappings()
        {
            try
            {
                var mappings = _sceneMappingProxy.Fetch();

                if (mappings.Any())
                {
                    _repository.Purge();
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
        }

        public virtual string GetSceneName(int tvdbId, int seasonNumber = -1)
        {
            var mapping = _repository.FindByTvdbId(tvdbId);

            if (mapping == null) return null;

            return mapping.SceneName;
        }

        public virtual Nullable<Int32> GetTvDbId(string cleanName)
        {
            var mapping = _repository.FindByCleanTitle(cleanName);

            if (mapping == null)
                return null;

            return mapping.TvdbId;
        }


        public virtual string GetCleanName(int tvdbId)
        {
            var mapping = _repository.FindByTvdbId(tvdbId);

            if (mapping == null) return null;

            return mapping.CleanTitle;
        }

        public void Init()
        {
            if (!_repository.HasItems())
            {
                UpdateMappings();
            }
        }
    }
}
