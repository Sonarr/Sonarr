using System.Diagnostics;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Jobs
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

        public int DefaultInterval
        {
            get { return 1440; }
        }

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            _logProvider.Trim();
        }
    }
}