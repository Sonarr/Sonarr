using System;
using System.IO;

namespace NzbDrone.Common
{
    public static class PathExtentions
    {
        private const string WEB_FOLDER = "NzbDrone.Web\\";
        private const string APP_DATA = "App_Data\\";
        public const string IIS_FOLDER = "IISExpress";
        public const string IIS_EXE = "iisexpress.exe";


        private const string LOG_CONFIG_FILE = "log.config";
        private const string APP_CONFIG_FILE = "config.xml";

        public const string NZBDRONE_DB_FILE = "nzbdrone.sdf";
        public const string LOG_DB_FILE = "log.sdf";

        private const string BACKUP_ZIP_FILE = "NzbDrone_Backup.zip";

        private const string UPDATE_SANDBOX_FOLDER_NAME = "nzbdrone_update\\";
        private const string UPDATE_PACKAGE_FOLDER_NAME = "nzbdrone\\";
        private const string UPDATE_BACKUP_FOLDER_NAME = "nzbdrone_backup\\";
        private const string UPDATE_CLIENT_EXE = "nzbdrone.update.exe";
        private const string UPDATE_CLIENT_FOLDER_NAME = "NzbDrone.Update\\";
        private const string UPDATE_LOG_FOLDER_NAME = "UpdateLogs\\";

        public static string NormalizePath(this string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path can not be null or empty");

            var info = new FileInfo(path);

            if (info.FullName.StartsWith(@"\\")) //UNC
            {
                return info.FullName.TrimEnd('/', '\\', ' ');
            }

            return info.FullName.Trim('/', '\\', ' ');
        }


        public static string GetIISFolder(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.ApplicationPath, IIS_FOLDER);
        }

        public static string GetIISExe(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetIISFolder(), IIS_EXE);
        }

        public static string GetIISConfigPath(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetIISFolder(), "AppServer", "applicationhost.config");
        }

        public static string GetWebRoot(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.ApplicationPath, WEB_FOLDER);
        }

        public static string GetAppDataPath(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetWebRoot(), APP_DATA);
        }

        public static string GetNlogConfigPath(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetWebRoot(), LOG_CONFIG_FILE);
        }

        public static string GetConfigPath(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.ApplicationPath, APP_CONFIG_FILE);
        }

        public static string GetNzbDronoeDbFile(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), NZBDRONE_DB_FILE);
        }

        public static string GetLogDbFileDbFile(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), LOG_DB_FILE);
        }

        public static string GetMediaCoverPath(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetWebRoot(), "MediaCover");
        }

        public static string GetBannerPath(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetMediaCoverPath(), "Banners");
        }

        public static string GetFanArtPath(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetMediaCoverPath(), "Fanarts");
        }

        public static string GetCacheFolder(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetWebRoot(), "Cache");
        }

        public static string GetUpdateLogFolder(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.ApplicationPath, UPDATE_LOG_FOLDER_NAME);
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

        public static string GetLogFileName(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.ApplicationPath, "nzbdrone.log.txt");
        }

        public static string GetArchivedLogFileName(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.ApplicationPath, "nzbdrone.log.0.txt");
        }

        public static string GetConfigBackupFile(this EnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), BACKUP_ZIP_FILE);
        }
    }
}