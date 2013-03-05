using System;
using System.Linq;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Jobs.Framework;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Instrumentation
{
    public class TrimLogsJob : IJob
    {
        private readonly LogService _logService;

        public TrimLogsJob(LogService logService)
        {
            _logService = logService;
        }

        public string Name
        {
            get { return "Trim Logs  Job"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromDays(1); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            _logService.Trim();
        }
    }
}