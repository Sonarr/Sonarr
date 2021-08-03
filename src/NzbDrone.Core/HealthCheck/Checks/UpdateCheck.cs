using System;
using System.IO;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ConfigFileSavedEvent))]
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
            var startupFolder = _appFolderInfo.StartUpFolder;
            var uiFolder = Path.Combine(startupFolder, "UI");

            if ((OsInfo.IsWindows || _configFileProvider.UpdateAutomatically) &&
                _configFileProvider.UpdateMechanism == UpdateMechanism.BuiltIn)
            {
                if (OsInfo.IsOsx && startupFolder.GetAncestorFolders().Contains("AppTranslocation"))
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        string.Format("Cannot install update because startup folder '{0}' is in an App Translocation folder.", startupFolder),
                        "#cannot-install-update-because-startup-folder-is-in-an-app-translocation-folder");
                }

                if (!_diskProvider.FolderWritable(startupFolder))
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        string.Format("Cannot install update because startup folder '{0}' is not writable by the user '{1}'.", startupFolder, Environment.UserName),
                        "#cannot-install-update-because-startup-folder-is-not-writable-by-the-user");
                }

                if (!_diskProvider.FolderWritable(uiFolder))
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        string.Format("Cannot install update because UI folder '{0}' is not writable by the user '{1}'.", uiFolder, Environment.UserName),
                        "#cannot-install-update-because-ui-folder-is-not-writable-by-the-user");
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
    }
}
