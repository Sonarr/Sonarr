using System.IO;
using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace NzbDrone.Common.Instrumentation
{
    [ThreadAgnostic]
    [LayoutRenderer("appLog")]
    public class ApplicationLogLayoutRenderer : LayoutRenderer
    {
        private readonly string _appData;

        public ApplicationLogLayoutRenderer()
        {
            _appData = Path.Combine(new EnvironmentProvider().GetLogFolder(), "nzbdrone.txt");

        }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(_appData);
        }
    }
}