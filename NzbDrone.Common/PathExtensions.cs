using System.IO;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Common
{
    public static class PathExtensions
    {
        private static readonly string APP_DATA = "App_Data" + Path.DirectorySeparatorChar;
        private static readonly string APP_CONFIG_FILE = "config.xml";
        private static readonly string NZBDRONE_DB = "nzbdrone.db";
        private static readonly string BACKUP_ZIP_FILE = "NzbDrone_Backup.zip";

        private static readonly string UPDATE_SANDBOX_FOLDER_NAME = "nzbdrone_update" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_PACKAGE_FOLDER_NAME = "nzbdrone" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_BACKUP_FOLDER_NAME = "nzbdrone_backup" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_CLIENT_EXE = "nzbdrone.update.exe";
        private static readonly string UPDATE_CLIENT_FOLDER_NAME = "NzbDrone.Update" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_LOG_FOLDER_NAME = "UpdateLogs" + Path.DirectorySeparatorChar;

        public static string CleanPath(this string path)
        {
            Ensure.That(() => path).IsNotNullOrWhiteSpace();

            var info = new FileInfo(path);

            if (info.FullName.StartsWith(@"\\")) //UNC
            {
                return info.FullName.TrimEnd('/', '\\', ' ');
            }

            return info.FullName.TrimEnd('/').Trim('\\', ' ');
        }

        static string GetProperDirectoryCapitalization(DirectoryInfo dirInfo)
        {
            var parentDirInfo = dirInfo.Parent;
            if (null == parentDirInfo)
                return dirInfo.Name;
            return Path.Combine(GetProperDirectoryCapitalization(parentDirInfo),
                                parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }

        static string GetProperFilePathCapitalization(string filename)
        {
            var fileInfo = new FileInfo(filename);
            DirectoryInfo dirInfo = fileInfo.Directory;
            return Path.Combine(GetProperDirectoryCapitalization(dirInfo),
                                dirInfo.GetFiles(fileInfo.Name)[0].Name);
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