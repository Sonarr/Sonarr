using System;
using System.Linq;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs.Implementations
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