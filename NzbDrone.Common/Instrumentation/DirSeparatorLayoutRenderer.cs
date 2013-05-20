using System.IO;
using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace NzbDrone.Common.Instrumentation
{
    [ThreadAgnostic]
    [LayoutRenderer("dirSeparator")]
    public class DirSeparatorLayoutRenderer : LayoutRenderer
    {

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(Path.DirectorySeparatorChar);
        }
    }
}