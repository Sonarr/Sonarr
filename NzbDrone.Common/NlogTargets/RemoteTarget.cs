using System.Linq;
using System.Diagnostics;
using NLog;
using NLog.Targets;

namespace NzbDrone.Common.NlogTargets
{
    public class RemoteTarget : Target
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent == null || logEvent.Exception == null) return;

            logger.Trace("Sending Exception to Service.NzbDrone.com . Process Name: {0}", Process.GetCurrentProcess().ProcessName);

            ReportingService.ReportException(logEvent);
        }
    }
}