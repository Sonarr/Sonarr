using System;
using System.Linq;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs.Implementations
{
    public class EmptyRecycleBinJob : IJob
    {
        private readonly RecycleBinProvider _recycleBinProvider;

        public EmptyRecycleBinJob(RecycleBinProvider recycleBinProvider)
        {
            _recycleBinProvider = recycleBinProvider;
        }

        public string Name
        {
            get { return "Empty Recycle Bin"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            _recycleBinProvider.Empty();
        }
    }
}