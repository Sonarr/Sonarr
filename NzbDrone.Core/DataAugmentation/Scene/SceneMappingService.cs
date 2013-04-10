using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    public interface ISceneMappingService
    {
        void UpdateMappings();
        string GetSceneName(int seriesId, int seasonNumber = -1);
        Nullable<int> GetTvDbId(string cleanName);
        string GetCleanName(int tvdbId);
    }

    public class SceneMappingService : ISceneMappingService,IHandleAsync<ApplicationStartedEvent>
    {
        private readonly ISceneMappingRepository _repository;
        private readonly ISceneMappingProxy _sceneMappingProxy;
        private readonly ISeriesService _seriesService;
        private readonly Logger _logger;

        public SceneMappingService(ISceneMappingRepository repository, ISceneMappingProxy sceneMappingProxy, ISeriesService seriesService, Logger logger)
        {
            _repository = repository;
            _sceneMappingProxy = sceneMappingProxy;
            _seriesService = seriesService;
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

        public string GetSceneName(int seriesId, int seasonNumber = -1)
        {
            var tvDbId = _seriesService.FindByTvdbId(seriesId).TvDbId;

            var mapping = _repository.FindByTvdbId(tvDbId);

            if (mapping == null) return null;

            return mapping.SceneName;
        }



        public Nullable<Int32> GetTvDbId(string cleanName)
        {
            var mapping = _repository.FindByCleanTitle(cleanName);

            if (mapping == null)
                return null;

            return mapping.TvdbId;
        }


        public string GetCleanName(int tvdbId)
        {
            var mapping = _repository.FindByTvdbId(tvdbId);

            if (mapping == null) return null;

            return mapping.CleanTitle;
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            if (!_repository.HasItems())
            {
                UpdateMappings();
            }
        }
    }
}
