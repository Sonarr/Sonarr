using System;
using System.IO;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Common
{
    public static class PathExtensions
    {
        private const string APP_DATA = "App_Data\\";
        private const string APP_CONFIG_FILE = "config.xml";
        private const string NZBDRONE_DB = "nzbdrone.db";
        private const string BACKUP_ZIP_FILE = "NzbDrone_Backup.zip";

        private const string UPDATE_SANDBOX_FOLDER_NAME = "nzbdrone_update\\";
        private const string UPDATE_PACKAGE_FOLDER_NAME = "nzbdrone\\";
        private const string UPDATE_BACKUP_FOLDER_NAME = "nzbdrone_backup\\";
        private const string UPDATE_CLIENT_EXE = "nzbdrone.update.exe";
        private const string UPDATE_CLIENT_FOLDER_NAME = "NzbDrone.Update\\";
        private const string UPDATE_LOG_FOLDER_NAME = "UpdateLogs\\";

        public static string NormalizePath(this string path)
        {
            Ensure.That(() => path).IsNotNullOrWhiteSpace();

            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path can not be null or empty");

            var info = new FileInfo(path);

            if (info.FullName.StartsWith(@"\\")) //UNC
            {
                return info.FullName.TrimEnd('/', '\\', ' ');
            }

            return info.FullName.Trim('/', '\\', ' ');
        }

        public static string GetAppDataPath(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.WorkingDirectory, APP_DATA);
        }

        public static string GetConfigPath(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.WorkingDirectory, APP_CONFIG_FILE);
        }

        public static string GetMediaCoverPath(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), "MediaCover");
        }

        public static string GetUpdateLogFolder(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.WorkingDirectory, UPDATE_LOG_FOLDER_NAME);
        }

        public static string GetUpdateSandboxFolder(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.SystemTemp, UPDATE_SANDBOX_FOLDER_NAME);
        }

        public static string GetUpdateBackUpFolder(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetUpdateSandboxFolder(), UPDATE_BACKUP_FOLDER_NAME);
        }

        public static string GetUpdatePackageFolder(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetUpdateSandboxFolder(), UPDATE_PACKAGE_FOLDER_NAME);
        }

        public static string GetUpdateClientFolder(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetUpdatePackageFolder(), UPDATE_CLIENT_FOLDER_NAME);
        }

        public static string GetUpdateClientExePath(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetUpdateSandboxFolder(), UPDATE_CLIENT_EXE);
        }

        public static string GetSandboxLogFolder(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetUpdateSandboxFolder(), UPDATE_LOG_FOLDER_NAME);
        }

        public static string GetConfigBackupFile(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), BACKUP_ZIP_FILE);
        }

        public static string GetNzbDroneDatabase(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), NZBDRONE_DB);
        }
    }
}