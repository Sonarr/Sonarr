using System;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Update
{
    public class AppUpdateJob : IJob
    {
        private readonly IUpdateService _updateService;

        public AppUpdateJob(IUpdateService updateService)
        {
            _updateService = updateService;
        }

        public string Name
        {
            get { return "Update Application Job"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromDays(2); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            _updateService.InstallAvailableUpdate();
        }
    }
}