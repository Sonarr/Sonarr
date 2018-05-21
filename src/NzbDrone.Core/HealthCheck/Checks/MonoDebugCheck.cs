using System.Diagnostics;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MonoDebugCheck : HealthCheckBase
    {
        private readonly Logger _logger;
        private readonly StackFrameHelper _stackFrameHelper;

        public override bool CheckOnSchedule => false;

        public MonoDebugCheck(Logger logger, StackFrameHelper stackFrameHelper)
        {
            _logger = logger;
            _stackFrameHelper = stackFrameHelper;
        }

        public class StackFrameHelper
        {
            public virtual bool HasStackFrameInfo()
            {
                var stackTrace = new StackTrace(true);

                return stackTrace.FrameCount > 0 && stackTrace.GetFrame(0).GetFileName().IsNotNullOrWhiteSpace();
            }
        }

        public override HealthCheck Check()
        {
            if (!PlatformInfo.IsMono)
            {
                return new HealthCheck(GetType());
            }

            if (!_stackFrameHelper.HasStackFrameInfo())
            {
                _logger.Warn("Mono is not running with --debug switch");
                return new HealthCheck(GetType());
            }

            return new HealthCheck(GetType());
        }
    }
}
