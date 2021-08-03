using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Exceptions;
using NzbDrone.Common.Extensions;
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
        private readonly IStartupContext _startupContext;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly Logger _logger;

        public AppFolderFactory(IAppFolderInfo appFolderInfo,
                                IStartupContext startupContext,
                                IDiskProvider diskProvider,
                                IDiskTransferService diskTransferService)
        {
            _appFolderInfo = appFolderInfo;
            _startupContext = startupContext;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _logger = NzbDroneLogger.GetLogger(this);
        }

        public void Register()
        {
            try
            {
                MigrateAppDataFolder();
                _diskProvider.EnsureFolder(_appFolderInfo.AppDataFolder);
            }
            catch (UnauthorizedAccessException)
            {
                throw new SonarrStartupException("Cannot create AppFolder, Access to the path {0} is denied", _appFolderInfo.AppDataFolder);
            }

            if (OsInfo.IsWindows)
            {
                SetPermissions();
            }

            if (!_diskProvider.FolderWritable(_appFolderInfo.AppDataFolder))
            {
                throw new SonarrStartupException("AppFolder {0} is not writable", _appFolderInfo.AppDataFolder);
            }

            InitializeMonoApplicationData();
        }

        private void SetPermissions()
        {
            try
            {
                _diskProvider.SetEveryonePermissions(_appFolderInfo.AppDataFolder);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Coudn't set app folder permission");
            }
        }

        private void MigrateAppDataFolder()
        {
            try
            {
                var oldDbFile = Path.Combine(_appFolderInfo.AppDataFolder, "nzbdrone.db");

                if (_startupContext.Args.ContainsKey(StartupContext.APPDATA))
                {
                    if (_diskProvider.FileExists(_appFolderInfo.GetDatabase()))
                    {
                        return;
                    }

                    if (!_diskProvider.FileExists(oldDbFile))
                    {
                        return;
                    }

                    MoveSqliteDatabase(oldDbFile, _appFolderInfo.GetDatabase());
                    RemovePidFile();
                }

                if (_appFolderInfo.LegacyAppDataFolder.IsNullOrWhiteSpace())
                {
                    return;
                }

                if (_diskProvider.FileExists(_appFolderInfo.GetDatabase()) || _diskProvider.FileExists(_appFolderInfo.GetConfigPath()))
                {
                    return;
                }

                if (!_diskProvider.FolderExists(_appFolderInfo.LegacyAppDataFolder))
                {
                    return;
                }

                // Delete the bin folder on Windows
                var binFolder = Path.Combine(_appFolderInfo.LegacyAppDataFolder, "bin");

                if (OsInfo.IsWindows && _diskProvider.FolderExists(binFolder))
                {
                    _diskProvider.DeleteFolder(binFolder, true);
                }

                // Transfer other files and folders (with copy so a backup is maintained)
                _diskTransferService.TransferFolder(_appFolderInfo.LegacyAppDataFolder, _appFolderInfo.AppDataFolder, TransferMode.Copy);

                // Rename the DB file
                if (_diskProvider.FileExists(oldDbFile))
                {
                    MoveSqliteDatabase(oldDbFile, _appFolderInfo.GetDatabase());
                }

                // Remove Old PID file
                RemovePidFile();

                // Delete the old files after everything has been copied
                _diskProvider.DeleteFolder(_appFolderInfo.LegacyAppDataFolder, true);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, ex.Message);
                throw new SonarrStartupException("Unable to migrate AppData folder from {0} to {1}. Migrate manually", _appFolderInfo.LegacyAppDataFolder, _appFolderInfo.AppDataFolder);
            }
        }

        private void InitializeMonoApplicationData()
        {
            if (OsInfo.IsWindows)
            {
                return;
            }

            try
            {
                // It seems that DoNotVerify is the mono behaviour even though .net docs specify a blank string
                // should be returned if the data doesn't exist.  For compatibility with .net core, explicitly
                // set DoNotVerify (which makes sense given we're explicitly checking that the folder exists)
                var configHome = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify);
                if (configHome.IsNullOrWhiteSpace() ||
                    configHome == "/.config" ||
                    (configHome.EndsWith("/.config") && !_diskProvider.FolderExists(configHome.GetParentPath())) ||
                    !_diskProvider.FolderExists(configHome))
                {
                    // Tell mono/netcore to use appData/.config as ApplicationData folder.
                    Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", Path.Combine(_appFolderInfo.AppDataFolder, ".config"));
                }

                var dataHome = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
                if (dataHome.IsNullOrWhiteSpace() ||
                    dataHome == "/.local/share" ||
                    (dataHome.EndsWith("/.local/share") && !_diskProvider.FolderExists(dataHome.GetParentPath().GetParentPath())) ||
                    !_diskProvider.FolderExists(dataHome))
                {
                    // Tell mono/netcore to use appData/.config/share as LocalApplicationData folder.
                    Environment.SetEnvironmentVariable("XDG_DATA_HOME", Path.Combine(_appFolderInfo.AppDataFolder, ".config/share"));
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to initialize the mono config directory.");
            }
        }

        private void MoveSqliteDatabase(string source, string destination)
        {
            _logger.Info("Moving {0}* to {1}*", source, destination);

            var dbSuffixes = new[] { "", "-shm", "-wal", "-journal" };

            foreach (var suffix in dbSuffixes)
            {
                var sourceFile = source + suffix;
                var destFile = destination + suffix;

                if (_diskProvider.FileExists(destFile))
                {
                    _diskProvider.DeleteFile(destFile);
                }

                if (_diskProvider.FileExists(sourceFile))
                {
                    _diskProvider.CopyFile(sourceFile, destFile);
                }
            }

            foreach (var suffix in dbSuffixes)
            {
                var sourceFile = source + suffix;

                if (_diskProvider.FileExists(sourceFile))
                {
                    _diskProvider.DeleteFile(sourceFile);
                }
            }
        }

        private void RemovePidFile()
        {
            if (OsInfo.IsNotWindows)
            {
                _diskProvider.DeleteFile(Path.Combine(_appFolderInfo.AppDataFolder, "sonarr.pid"));
            }
        }
    }
}
