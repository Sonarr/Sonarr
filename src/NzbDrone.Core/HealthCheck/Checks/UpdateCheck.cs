using System;
using System.IO;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class UpdateCheck : HealthCheckBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly ICheckUpdateService _checkUpdateService;
        private readonly IConfigFileProvider _configFileProvider;

        public UpdateCheck(IDiskProvider diskProvider,
                           IAppFolderInfo appFolderInfo,
                           ICheckUpdateService checkUpdateService,
                           IConfigFileProvider configFileProvider)
        {
            _diskProvider = diskProvider;
            _appFolderInfo = appFolderInfo;
            _checkUpdateService = checkUpdateService;
            _configFileProvider = configFileProvider;
        }

        public override HealthCheck Check()
        {
            if ((OsInfo.IsWindows || _configFileProvider.UpdateAutomatically) &&
                _configFileProvider.UpdateMechanism == UpdateMechanism.BuiltIn)
            {
                if (!_diskProvider.FolderWritable(_appFolderInfo.StartUpFolder))
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Error,
                        string.Format("Cannot install update because startup folder '{0}' is not writable by the user '{1}'.", _appFolderInfo.StartUpFolder, Environment.UserName),
                        "Cannot install update because startup folder is not writable by the user");
                }
            }

            if (BuildInfo.BuildDateTime < DateTime.UtcNow.AddDays(-14))
            {
                if (_checkUpdateService.AvailableUpdate() != null)
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Warning, "New update is available");
                }
            }

            return new HealthCheck(GetType());
        }

        public override bool CheckOnConfigChange
        {
            get
            {
                return false;
            }
        }
    }
}
