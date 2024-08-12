using System.Text;
using NLog;
using NLog.Targets;

namespace NzbDrone.Common.Instrumentation
{
    public class NzbDroneFileTarget : FileTarget
    {
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            var result = CleanseLogMessage.Cleanse(Layout.Render(logEvent));
            target.Append(result);
        }
    }
}
