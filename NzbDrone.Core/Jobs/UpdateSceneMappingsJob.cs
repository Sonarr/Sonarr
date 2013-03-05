using System;
using System.Linq;
using NzbDrone.Core.Jobs.Framework;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.ReferenceData;

namespace NzbDrone.Core.Jobs
{
    public class UpdateSceneMappingsJob : IJob
    {
        private readonly SceneMappingService _sceneNameMappingService;

        public UpdateSceneMappingsJob(SceneMappingService sceneNameMappingService)
        {
            _sceneNameMappingService = sceneNameMappingService;
        }

        public UpdateSceneMappingsJob()
        {

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