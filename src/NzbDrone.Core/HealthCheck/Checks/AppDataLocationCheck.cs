using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class AppDataLocationCheck : HealthCheckBase
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public AppDataLocationCheck(IAppFolderInfo appFolderInfo)
        {
            _appFolderInfo = appFolderInfo;
        }

        public override HealthCheck Check()
        {
            if (_appFolderInfo.StartUpFolder.IsParentPath(_appFolderInfo.AppDataFolder) ||
                _appFolderInfo.StartUpFolder.PathEquals(_appFolderInfo.AppDataFolder))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Updating will not be possible to prevent deleting AppData on Update", "#updating-will-not-be-possible-to-prevent-deleting-appdata-on-update");
            }

            return new HealthCheck(GetType());
        }
    }
}
