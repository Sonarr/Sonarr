using System;
using System.Linq;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Jobs
{
    public class TrimLogsJob : IJob
    {
        private readonly LogProvider _logProvider;

        public TrimLogsJob(LogProvider logProvider)
        {
            _logProvider = logProvider;
        }

        public string Name
        {
            get { return "Trim Logs  Job"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromDays(1); }
        }

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            _logProvider.Trim();
        }
    }
}