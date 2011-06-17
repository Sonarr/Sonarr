using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Jobs
{
    public class UpdateSceneMappingsJob : IJob
    {
        private readonly SceneMappingProvider _sceneNameMappingProvider;

        public UpdateSceneMappingsJob(SceneMappingProvider sceneNameMappingProvider)
        {
            _sceneNameMappingProvider = sceneNameMappingProvider;
        }

        public UpdateSceneMappingsJob()
        {
            
        }

        public string Name
        {
            get { return "Update Scene Mappings"; }
        }

        public int DefaultInterval
        {
            get { return 720; } //Every 12 hours
        }

        public virtual void Start(ProgressNotification notification, int targetId)
        {
            notification.CurrentMessage = "Updating Scene Mappings";
            if (_sceneNameMappingProvider.UpdateMappings())
                notification.CurrentMessage = "Scene Mappings Completed";

            else
                notification.CurrentMessage = "Scene Mappings Failed";
        }
    }
}