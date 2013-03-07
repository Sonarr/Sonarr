using System.Linq;
using System;
using NLog;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Core.ReferenceData
{
    public interface ISceneMappingService
    {
        void UpdateMappings();
        string GetSceneName(int tvdbId, int seasonNumber = -1);
        Nullable<Int32> GetTvDbId(string cleanName);
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

                _repository.Purge();
                _repository.InsertMany(mappings);
            }
            catch (Exception ex)
            {
                _logger.InfoException("Failed to Update Scene Mappings:", ex);
            }
        }

        public virtual string GetSceneName(int tvdbId, int seasonNumber = -1)
        {
            var mapping = _repository.FindByTvdbId(tvdbId);

            if(mapping == null) return null;

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
