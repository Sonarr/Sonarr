using System.Text;
using NLog;
using NLog.Config;
using NLog.Internal;
using NLog.LayoutRenderers;

namespace NzbDrone.Common.Instrumentation.Sentry
{
    [ThreadAgnostic]
    [LayoutRenderer("populatestacktrace")]
    public class PopulateStackTraceRenderer : LayoutRenderer, IUsesStackTrace
    {
        StackTraceUsage IUsesStackTrace.StackTraceUsage => StackTraceUsage.WithSource;

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            // This is only used to populate the stacktrace.  doesn't actually render anything.
        }
    }
}
