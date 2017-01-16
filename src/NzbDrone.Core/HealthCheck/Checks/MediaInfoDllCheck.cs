using System;
using System.Runtime.CompilerServices;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MediaInfoDllCheck : HealthCheckBase
    {
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public override HealthCheck Check()
        {
            try
            {
                var mediaInfo = new MediaInfo();
            }
            catch (Exception e)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, $"MediaInfo Library could not be loaded {e.Message}");
            }

            return new HealthCheck(GetType());
        }

        public override bool CheckOnConfigChange => false;
    }
}
