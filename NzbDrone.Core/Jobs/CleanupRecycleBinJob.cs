using System.Linq;
using System;
using NLog;
using NzbDrone.Core.Jobs.Framework;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Converting;

namespace NzbDrone.Core.Jobs
{
    public class CleanupRecycleBinJob : IJob
    {
        private readonly RecycleBinProvider _recycleBinProvider;

        public CleanupRecycleBinJob(RecycleBinProvider recycleBinProvider)
        {
            _recycleBinProvider = recycleBinProvider;
        }

        public string Name
        {
            get { return "Cleanup Recycle Bin"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromDays(24); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            _recycleBinProvider.Cleanup();
        }
    }
}