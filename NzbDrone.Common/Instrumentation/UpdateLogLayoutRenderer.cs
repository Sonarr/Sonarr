using System;
using System.IO;
using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Instrumentation
{
    [ThreadAgnostic]
    [LayoutRenderer("updateLog")]
    public class UpdateLogLayoutRenderer : LayoutRenderer
    {
        private readonly string _appData;

        public UpdateLogLayoutRenderer()
        {
            _appData = Path.Combine(new AppDirectoryInfo().GetUpdateLogFolder(), DateTime.Now.ToString("yy.MM.d-HH.mm") + ".txt");

        }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(_appData);
        }
    }
}