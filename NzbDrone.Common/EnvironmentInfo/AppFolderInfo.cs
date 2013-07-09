using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using NLog;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IAppFolderInfo
    {
        string AppDataFolder { get; }
        string TempFolder { get; }
        string StartUpFolder { get; }
    }

    public class AppFolderInfo : IAppFolderInfo
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public AppFolderInfo(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
            _logger = LogManager.GetCurrentClassLogger();
            AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.DoNotVerify), "NzbDrone");
            StartUpFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            TempFolder = Path.GetTempPath();

            if (!_diskProvider.FolderExists(AppDataFolder))
            {
                MigrateFromAppData();
            }

            SetPermissions();
        }

        private void MigrateFromAppData()
        {
            var oldAppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "NzbDrone");

            if (_diskProvider.FolderExists(oldAppDataFolder))
            {
                _diskProvider.MoveFolder(oldAppDataFolder, AppDataFolder);
            }
            else
            {
                _diskProvider.CreateFolder(AppDataFolder);
            }
        }

        private void SetPermissions()
        {
            try
            {
                _diskProvider.SetPermissions(AppDataFolder, "Everyone", FileSystemRights.FullControl, AccessControlType.Allow);
            }
            catch (Exception ex)
            {
                _logger.WarnException("Coudn't set app folder permission", ex);
            }
        }

        public string AppDataFolder { get; private set; }

        public string StartUpFolder { get; private set; }

        public String TempFolder { get; private set; }
    }
}