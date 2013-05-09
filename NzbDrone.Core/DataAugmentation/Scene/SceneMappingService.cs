using System;
using System.Linq;
using NLog;
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

        public SceneMappingService(ISceneMappingRepository repository, ISceneMappingProxy sceneMappingProxy, Logger logger)
        {
            _repository = repository;
            _sceneMappingProxy = sceneMappingProxy;
            _logger = logger;
        }

        public string GetSceneName(int tvdbId, int seasonNumber = -1)
        {
            var mapping = _repository.FindByTvdbId(tvdbId);

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
