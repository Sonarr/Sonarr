using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers.Jobs
{
    public class AppUpdateJob : IJob
    {
        private readonly UpdateProvider _updateProvider;

        public AppUpdateJob(UpdateProvider updateProvider)
        {
            _updateProvider = updateProvider;
        }

        public string Name
        {
            get { return "Update Application Job"; }
        }

        public int DefaultInterval
        {
            get { return 0; }
        }

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            notification.CurrentMessage = "Updating NzbDrone";

            var updatePackage = _updateProvider.GetAvilableUpdate();

            _updateProvider.StartUpdate(updatePackage);
        }
    }
}