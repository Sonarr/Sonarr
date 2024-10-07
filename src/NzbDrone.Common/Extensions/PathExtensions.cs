using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Extensions
{
    public static class PathExtensions
    {
        private const string APP_CONFIG_FILE = "config.xml";
        private const string DB = "sonarr.db";
        private const string DB_RESTORE = "sonarr.restore";
        private const string LOG_DB = "logs.db";
        private const string NLOG_CONFIG_FILE = "nlog.config";
        private const string UPDATE_CLIENT_EXE_NAME = "Sonarr.Update";

        private static readonly string UPDATE_SANDBOX_FOLDER_NAME = "sonarr_update" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_PACKAGE_FOLDER_NAME = "Sonarr" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_BACKUP_FOLDER_NAME = "sonarr_backup" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_BACKUP_APPDATA_FOLDER_NAME = "sonarr_appdata_backup" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_CLIENT_FOLDER_NAME = "Sonarr.Update" + Path.DirectorySeparatorChar;
        private static readonly string UPDATE_LOG_FOLDER_NAME = "UpdateLogs" + Path.DirectorySeparatorChar;

        public static string CleanFilePath(this string path)
        {
            if (path.IsNotNullOrWhiteSpace())
            {
                // Trim trailing spaces before checking if the path is valid so validation doesn't fail for something we can fix.
                path = path.TrimEnd(' ');
            }

            Ensure.That(path, () => path).IsNotNullOrWhiteSpace();
            Ensure.That(path, () => path).IsValidPath(PathValidationType.AnyOs);

            var info = new FileInfo(path.Trim());

            // UNC
            if (!info.FullName.Contains('/') && info.FullName.StartsWith(@"\\"))
            {
                return info.FullName.TrimEnd('/', '\\');
            }

            return info.FullName.TrimEnd('/').Trim('\\');
        }

        public static bool PathNotEquals(this string firstPath, string secondPath, StringComparison? comparison = null)
        {
            return !PathEquals(firstPath, secondPath, comparison);
        }

        public static bool PathEquals(this string firstPath, string secondPath, StringComparison? comparison = null)
        {
            if (!comparison.HasValue)
            {
                comparison = DiskProviderBase.PathStringComparison;
            }

            if (firstPath.Equals(secondPath, comparison.Value))
            {
                return true;
            }

            return string.Equals(firstPath.CleanFilePath(), secondPath.CleanFilePath(), comparison.Value);
        }

        public static string GetPathExtension(this string path)
        {
            var idx = path.LastIndexOf('.');
            if (idx == -1 || idx == path.Length - 1)
            {
                return string.Empty;
            }

            return path.Substring(idx);
        }

        public static string GetRelativePath(this string parentPath, string childPath)
        {
            if (!parentPath.IsParentPath(childPath))
            {
                throw new NotParentException("{0} is not a child of {1}", childPath, parentPath);
            }

            return childPath.Substring(parentPath.Length).Trim(Path.DirectorySeparatorChar);
        }

        public static string GetParentPath(this string childPath)
        {
            var path = new OsPath(childPath).Directory;

            return path == OsPath.Null ? null : path.PathWithoutTrailingSlash;
        }

        public static string GetParentName(this string childPath)
        {
            var path = new OsPath(childPath).Directory;

            return path == OsPath.Null ? null : path.Name;
        }

        public static string GetDirectoryName(this string childPath)
        {
            var path = new OsPath(childPath);

            return path == OsPath.Null ? null : path.Name;
        }

        public static string GetCleanPath(this string path)
        {
            var osPath = new OsPath(path);

            return osPath == OsPath.Null ? null : osPath.PathWithoutTrailingSlash;
        }

        public static bool IsParentPath(this string parentPath, string childPath)
        {
            var parent = new OsPath(parentPath);
            var child = new OsPath(childPath);

            while (child.Directory != OsPath.Null)
            {
                if (child.Directory.Equals(parent, true))
                {
                    return true;
                }

                child = child.Directory;
            }

            return false;
        }

        private static readonly Regex WindowsPathWithDriveRegex = new Regex(@"^[a-zA-Z]:\\", RegexOptions.Compiled);

        public static bool IsPathValid(this string path, PathValidationType validationType)
        {
            if (string.IsNullOrWhiteSpace(path) || path.ContainsInvalidPathChars())
            {
                return false;
            }

            // Only check for leading or trailing spaces for path when running on Windows.
            if (OsInfo.IsWindows)
            {
                if (path.Trim() != path)
                {
                    return false;
                }

                var directoryInfo = new DirectoryInfo(path);

                while (directoryInfo != null)
                {
                    if (directoryInfo.Name.Trim() != directoryInfo.Name)
                    {
                        return false;
                    }

                    directoryInfo = directoryInfo.Parent;
                }
            }

            if (validationType == PathValidationType.AnyOs)
            {
                return IsPathValidForWindows(path) || IsPathValidForNonWindows(path);
            }

            if (OsInfo.IsNotWindows)
            {
                return IsPathValidForNonWindows(path);
            }

            return IsPathValidForWindows(path);
        }

        public static bool ContainsInvalidPathChars(this string text)
        {
            if (text.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(text));
            }

            return text.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
        }

        private static string GetProperCapitalization(DirectoryInfo dirInfo)
        {
            var parentDirInfo = dirInfo.Parent;
            if (parentDirInfo == null)
            {
                // Drive letter
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
            if (OsInfo.IsNotWindows || path.StartsWith("\\"))
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

        public static List<string> GetAncestorFolders(this string path)
        {
            var directory = new DirectoryInfo(path);
            var directories = new List<string>();

            while (directory != null)
            {
                directories.Insert(0, directory.Name);

                directory = directory.Parent;
            }

            return directories;
        }

        public static string GetAncestorPath(this string path, string ancestorName)
        {
            var parent = Path.GetDirectoryName(path);

            while (parent != null)
            {
                var currentPath = parent;
                parent = Path.GetDirectoryName(parent);

                if (Path.GetFileName(currentPath) == ancestorName)
                {
                    return currentPath;
                }
            }

            return null;
        }

        public static string GetLongestCommonPath(this List<string> paths)
        {
            var firstPath = paths.First();
            var length = firstPath.Length;

            for (var i = 1; i < paths.Count; i++)
            {
                var path = paths[i];

                length = Math.Min(length, path.Length);

                for (var characterIndex = 0; characterIndex < length; characterIndex++)
                {
                    if (path[characterIndex] != firstPath[characterIndex])
                    {
                        length = characterIndex;
                        break;
                    }
                }
            }

            var substring = firstPath.Substring(0, length);
            var lastSeparatorIndex = substring.LastIndexOfAny(new[] { '/', '\\' });

            return substring.Substring(0, lastSeparatorIndex);
        }

        public static string ProcessNameToExe(this string processName)
        {
            if (OsInfo.IsWindows)
            {
                processName += ".exe";
            }

            return processName;
        }

        public static string CleanPath(this string path)
        {
            return Path.Join(path.Split(Path.DirectorySeparatorChar).Select(s => s.Trim()).ToArray());
        }

        public static string GetAppDataPath(this IAppFolderInfo appFolderInfo)
        {
            return appFolderInfo.AppDataFolder;
        }

        public static string GetDataProtectionPath(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetAppDataPath(appFolderInfo), "asp");
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

        public static string GetUpdateBackUpAppDataFolder(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetUpdateSandboxFolder(appFolderInfo), UPDATE_BACKUP_APPDATA_FOLDER_NAME);
        }

        public static string GetUpdateBackupConfigFile(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetUpdateBackUpAppDataFolder(appFolderInfo), APP_CONFIG_FILE);
        }

        public static string GetUpdateBackupDatabase(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetUpdateBackUpAppDataFolder(appFolderInfo), DB);
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
            return Path.Combine(GetUpdateSandboxFolder(appFolderInfo), UPDATE_CLIENT_EXE_NAME).ProcessNameToExe();
        }

        public static string GetDatabase(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetAppDataPath(appFolderInfo), DB);
        }

        public static string GetDatabaseRestore(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetAppDataPath(appFolderInfo), DB_RESTORE);
        }

        public static string GetLogDatabase(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(GetAppDataPath(appFolderInfo), LOG_DB);
        }

        public static string GetNlogConfigPath(this IAppFolderInfo appFolderInfo)
        {
            return Path.Combine(appFolderInfo.StartUpFolder, NLOG_CONFIG_FILE);
        }

        private static bool IsPathValidForWindows(string path)
        {
            return path.StartsWith("\\") || WindowsPathWithDriveRegex.IsMatch(path);
        }

        private static bool IsPathValidForNonWindows(string path)
        {
            return path.StartsWith("/");
        }
    }
}
