using System;
using System.Linq;
using NzbDrone.Core.DataAugmentation;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Jobs.Implementations
{
    public class UpdateSceneMappingsJob : IJob
    {
        private readonly SceneMappingService _sceneNameMappingService;

        public UpdateSceneMappingsJob(SceneMappingService sceneNameMappingService)
        {
            _sceneNameMappingService = sceneNameMappingService;
        }

        public string Name
        {
            get { return "Update Scene Mappings"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromHours(6); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            _sceneNameMappingService.UpdateMappings();
        }
    }
}