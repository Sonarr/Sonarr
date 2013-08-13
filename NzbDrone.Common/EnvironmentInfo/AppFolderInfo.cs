using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
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
        private readonly Environment.SpecialFolder DATA_SPECIAL_FOLDER = Environment.SpecialFolder.CommonApplicationData;


        public AppFolderInfo(IDiskProvider diskProvider, IStartupArguments startupArguments)
        {
            _diskProvider = diskProvider;

            if (OsInfo.IsLinux)
            {
                DATA_SPECIAL_FOLDER = Environment.SpecialFolder.ApplicationData;
            }

            _logger = LogManager.GetCurrentClassLogger();

            if (startupArguments.Args.ContainsKey(StartupArguments.APPDATA))
            {
                AppDataFolder = startupArguments.Args[StartupArguments.APPDATA];
            }
            else
            {
                AppDataFolder = Path.Combine(Environment.GetFolderPath(DATA_SPECIAL_FOLDER, Environment.SpecialFolderOption.None), "NzbDrone");
            }

            _diskProvider.EnsureFolder(AppDataFolder);

            StartUpFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            TempFolder = Path.GetTempPath();

            diskProvider.EnsureFolder(AppDataFolder);

            if (!OsInfo.IsLinux)
            {
                SetPermissions();
            }
        }

        private void SetPermissions()
        {
            try
            {
                _diskProvider.SetPermissions(AppDataFolder, WellKnownSidType.WorldSid, FileSystemRights.FullControl, AccessControlType.Allow);
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