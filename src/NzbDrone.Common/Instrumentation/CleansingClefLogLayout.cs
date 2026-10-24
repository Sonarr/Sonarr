using System.Text;
using NLog;
using NLog.Layouts.ClefJsonLayout;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Instrumentation;

public class CleansingClefLogLayout : CompactJsonLayout
{
    protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
    {
        if (!RuntimeInfo.IsProduction)
        {
            base.RenderFormattedMessage(logEvent, target);

            return;
        }

        var result = CleanseLogMessage.Cleanse(logEvent, RenderUncleansed);
        target.Append(result);
    }

    private string RenderUncleansed(LogEventInfo logEvent)
    {
        var builder = new StringBuilder();
        base.RenderFormattedMessage(logEvent, builder);

        return builder.ToString();
    }
}
