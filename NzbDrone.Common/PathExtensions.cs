using System.IO;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common
{
    public static class PathExtensions
    {
        private const string APP_CONFIG_FILE = "config.xml";
        private const string NZBDRONE_DB = "nzbdrone.db";
        private const string NZBDRONE_LOG_DB = "logs.db";
        private const string BACKUP_ZIP_FILE = "NzbDrone_Backup.zip";
        private const string NLOG_CONFIG_FILE = "nlog.config";
        private const string UPDATE_CLIENT_EXE = "nzbdrone.update.exe";

        private static readonly string UPDATE_SANDBOX_FOLDER_NAME = "nzbdrone_update" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_PACKAGE_FOLDER_NAME = "nzbdrone" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_BACKUP_FOLDER_NAME = "nzbdrone_backup" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_CLIENT_FOLDER_NAME = "NzbDrone.Update" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_LOG_FOLDER_NAME = "UpdateLogs" + Path.DirectorySeparatorChar;

        public static string CleanFilePath(this string path)
        {
            Ensure.That(() => path).IsNotNullOrWhiteSpace();
            Ensure.That(() => path).IsValidPath();

            var info = new FileInfo(path.Trim());

            if (!OsInfo.IsLinux && info.FullName.StartsWith(@"\\")) //UNC
            {
                return info.FullName.TrimEnd('/', '\\', ' ');
            }

            return info.FullName.TrimEnd('/').Trim('\\', ' ');
        }


        public static bool ContainsInvalidPathChars(this string text)
        {
            return text.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
        }

        private static string GetProperCapitalization(DirectoryInfo dirInfo)
        {
            var parentDirInfo = dirInfo.Parent;
            if (parentDirInfo == null)
            {
                //Drive letter
                return dirInfo.Name.ToUpper();
            }
            return Path.Combine(GetProperCapitalization(parentDirInfo), parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }

        public static string GetActualCasing(this string path)
        {
            var attributes = File.GetAttributes(path);

            if (OsInfo.IsLinux || path.StartsWith("\\"))
            {
                return path;
            }

            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return GetProperCapitalization(new DirectoryInfo(path));
            }

            var fileInfo = new FileInfo(path);


            DirectoryInfo dirInfo = fileInfo.Directory;
            return Path.Combine(GetProperCapitalization(dirInfo), dirInfo.GetFiles(fileInfo.Name)[0].Name);
        }

        public static string GetAppDataPath(this IAppFolderInfo appFolderInfo)
        {
            return appFolderInfo.AppDataFolder;
        }

        public static string GetLogFolder(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetAppDataPath(appFolderInfo), "logs");
        }

        public static string GetConfigPath(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetAppDataPath(appFolderInfo), APP_CONFIG_FILE);
        }

        public static string GetMediaCoverPath(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetAppDataPath(appFolderInfo), "MediaCover");
        }

        public static string GetUpdateLogFolder(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetAppDataPath(appFolderInfo), UPDATE_LOG_FOLDER_NAME);
        }

        public static string GetUpdateSandboxFolder(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(appFolderInfo.TempFolder, UPDATE_SANDBOX_FOLDER_NAME);
        }

        public static string GetUpdateBackUpFolder(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetUpdateSandboxFolder(appFolderInfo), UPDATE_BACKUP_FOLDER_NAME);
        }

        public static string GetUpdatePackageFolder(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetUpdateSandboxFolder(appFolderInfo), UPDATE_PACKAGE_FOLDER_NAME);
        }

        public static string GetUpdateClientFolder(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetUpdatePackageFolder(appFolderInfo), UPDATE_CLIENT_FOLDER_NAME);
        }

        public static string GetUpdateClientExePath(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetUpdateSandboxFolder(appFolderInfo), UPDATE_CLIENT_EXE);
        }

        public static string GetConfigBackupFile(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetAppDataPath(appFolderInfo), BACKUP_ZIP_FILE);
        }

        public static string GetNzbDroneDatabase(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetAppDataPath(appFolderInfo), NZBDRONE_DB);
        }

        public static string GetLogDatabase(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetAppDataPath(appFolderInfo), NZBDRONE_LOG_DB);
        }

        public static string GetNlogConfigPath(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(appFolderInfo.StartUpFolder, NLOG_CONFIG_FILE);
        }
    }
}