using System;
using System.IO;
using System.Text.RegularExpressions;
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
            Ensure.That(path, () => path).IsNotNullOrWhiteSpace();
            Ensure.That(path, () => path).IsValidPath();

            var info = new FileInfo(path.Trim());

            if (!OsInfo.IsLinux && info.FullName.StartsWith(@"\\")) //UNC
            {
                return info.FullName.TrimEnd('/', '\\', ' ');
            }

            return info.FullName.TrimEnd('/').Trim('\\', ' ');
        }

        public static bool PathEquals(this string firstPath, string secondPath)
        {
            if (OsInfo.IsLinux)
            {
                if (firstPath.Equals(secondPath)) return true;
                return String.Equals(firstPath.CleanFilePath(), secondPath.CleanFilePath());
            }

            if (firstPath.Equals(secondPath, StringComparison.OrdinalIgnoreCase)) return true;
            return String.Equals(firstPath.CleanFilePath(), secondPath.CleanFilePath(), StringComparison.OrdinalIgnoreCase);
        }

        private static readonly Regex WindowsPathWithDriveRegex = new Regex(@"^[a-zA-Z]:\\", RegexOptions.Compiled);

        public static bool IsPathValid(this string path)
        {
            if (path.ContainsInvalidPathChars() || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (OsInfo.IsLinux)
            {
                return path.StartsWith(Path.DirectorySeparatorChar.ToString());
            }

            if (path.StartsWith("\\") || WindowsPathWithDriveRegex.IsMatch(path))
            {
                return true;
            }

            return false;
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

            var folderName = dirInfo.Name;

            if (dirInfo.Exists)
            {
                folderName = parentDirInfo.GetDirectories(dirInfo.Name)[0].Name;
            }

            return Path.Combine(GetProperCapitalization(parentDirInfo), folderName);
        }

        public static string GetActualCasing(this string path)
        {
            if (OsInfo.IsLinux || path.StartsWith("\\"))
            {
                return path;
            }

            if (Directory.Exists(path) && (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return GetProperCapitalization(new DirectoryInfo(path));
            }

            var fileInfo = new FileInfo(path);
            var dirInfo = fileInfo.Directory;

            var fileName = fileInfo.Name;

            if (dirInfo != null && fileInfo.Exists)
            {
                fileName = dirInfo.GetFiles(fileInfo.Name)[0].Name;
            }

            return Path.Combine(GetProperCapitalization(dirInfo), fileName);
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