using NzbDrone.Core.Model.Notification;

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
            _sceneNameMappingProvider.UpdateMappings();
        }
    }
}