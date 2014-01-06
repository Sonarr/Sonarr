using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IAppFolderFactory
    {
        void Register();
    }

    public class AppFolderFactory : IAppFolderFactory
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public AppFolderFactory(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider)
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
            _logger = NzbDroneLogger.GetLogger(this);
        }

        public void Register()
        {
            _diskProvider.EnsureFolder(_appFolderInfo.AppDataFolder);

            if (!OsInfo.IsLinux)
            {
                SetPermissions();
            }
        }

        private void SetPermissions()
        {
            try
            {
                _diskProvider.SetPermissions(_appFolderInfo.AppDataFolder, WellKnownSidType.WorldSid, FileSystemRights.FullControl, AccessControlType.Allow);
            }
            catch (Exception ex)
            {
                _logger.WarnException("Coudn't set app folder permission", ex);
            }
        }
    }
}
