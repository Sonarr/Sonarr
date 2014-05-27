using NLog;
using NLog.Targets;

namespace NzbDrone.Common.Instrumentation
{
    public class NzbDroneFileTarget : FileTarget
    {
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return CleanseLogMessage.Cleanse(Layout.Render(logEvent));
        }
    }
}
