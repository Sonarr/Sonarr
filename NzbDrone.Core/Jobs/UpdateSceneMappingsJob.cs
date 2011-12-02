using System.Linq;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public abstract class UpdateSceneMappingsJob : IJob
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

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            _sceneNameMappingProvider.UpdateMappings();
        }
    }
}