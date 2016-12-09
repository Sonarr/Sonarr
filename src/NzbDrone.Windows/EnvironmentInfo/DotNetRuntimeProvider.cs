using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Windows.EnvironmentInfo
{
    public class DotNetRuntimeProvider : RuntimeInfoBase
    {
        public DotNetRuntimeProvider(Common.IServiceProvider serviceProvider, Logger logger)
            : base(serviceProvider, logger)
        {
        }

        public override string RuntimeVersion => Environment.Version.ToString();
    }
}
