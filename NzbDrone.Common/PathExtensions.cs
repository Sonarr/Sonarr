using System.IO;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Common
{
    public static class PathExtensions
    {
        private static readonly string APP_CONFIG_FILE = "config.xml";
        private static readonly string NZBDRONE_DB = "nzbdrone.db";
        private static readonly string NZBDRONE_LOG_DB = "logs.db";
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


        public static bool ContainsInvalidPathChars(this string text)
        {
            return text.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
        }

        private static string GetProperDirectoryCapitalization(DirectoryInfo dirInfo)
        {
            var parentDirInfo = dirInfo.Parent;
            if (null == parentDirInfo)
                return dirInfo.Name;
            return Path.Combine(GetProperDirectoryCapitalization(parentDirInfo),
                                parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }

        public static string GetActualCasing(this string filename)
        {
            var fileInfo = new FileInfo(filename);
            DirectoryInfo dirInfo = fileInfo.Directory;
            return Path.Combine(GetProperDirectoryCapitalization(dirInfo),
                                dirInfo.GetFiles(fileInfo.Name)[0].Name);
        }


        public static string GetAppDataPath(this IEnvironmentProvider environmentProvider)
        {
            return environmentProvider.WorkingDirectory;
        }

        public static string GetLogFolder(this IEnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), "logs");
        }

        public static string GetConfigPath(this IEnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), APP_CONFIG_FILE);
        }

        public static string GetMediaCoverPath(this IEnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), "MediaCover");
        }

        public static string GetUpdateLogFolder(this IEnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), UPDATE_LOG_FOLDER_NAME);
        }

        public static string GetUpdateSandboxFolder(this IEnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.SystemTemp, UPDATE_SANDBOX_FOLDER_NAME);
        }

        public static string GetUpdateBackUpFolder(this IEnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetUpdateSandboxFolder(), UPDATE_BACKUP_FOLDER_NAME);
        }

        public static string GetUpdatePackageFolder(this IEnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetUpdateSandboxFolder(), UPDATE_PACKAGE_FOLDER_NAME);
        }

        public static string GetUpdateClientFolder(this IEnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetUpdatePackageFolder(), UPDATE_CLIENT_FOLDER_NAME);
        }

        public static string GetUpdateClientExePath(this IEnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetUpdateSandboxFolder(), UPDATE_CLIENT_EXE);
        }

        public static string GetConfigBackupFile(this IEnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), BACKUP_ZIP_FILE);
        }

        public static string GetNzbDroneDatabase(this IEnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), NZBDRONE_DB);
        }

        public static string GetLogDatabase(this IEnvironmentProvider environmentProvider)
        {
            return Path.Combine(environmentProvider.GetAppDataPath(), NZBDRONE_LOG_DB);
        }
    }
}