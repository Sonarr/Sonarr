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

        private static readonly Regex PARENT_PATH_END_SLASH_REGEX = new Regex(@"(?<!:)\\$", RegexOptions.Compiled);

        public static string CleanFilePath(this string path)
        {
            Ensure.That(path, () => path).IsNotNullOrWhiteSpace();
            Ensure.That(path, () => path).IsValidPath();

            var info = new FileInfo(path.Trim());

            //UNC
            if (OsInfo.IsWindows && info.FullName.StartsWith(@"\\"))
            {
                return info.FullName.TrimEnd('/', '\\', ' ');
            }

            return info.FullName.TrimEnd('/').Trim('\\', ' ');
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
            var cleanPath = OsInfo.IsWindows
                ? PARENT_PATH_END_SLASH_REGEX.Replace(childPath, "")
                : childPath.TrimEnd(Path.DirectorySeparatorChar);

            if (cleanPath.IsNullOrWhiteSpace())
            {
                return null;
            }

            return Directory.GetParent(cleanPath)?.FullName;
        }

        public static string GetCleanPath(this string path)
        {
            var cleanPath = OsInfo.IsWindows
                ? PARENT_PATH_END_SLASH_REGEX.Replace(path, "")
                : path.TrimEnd(Path.DirectorySeparatorChar);

            return cleanPath;
        }

        public static bool IsParentPath(this string parentPath, string childPath)
        {
            if (parentPath != "/" && !parentPath.EndsWith(":\\"))
            {
                parentPath = parentPath.TrimEnd(Path.DirectorySeparatorChar);
            }

            if (childPath != "/" && !parentPath.EndsWith(":\\"))
            {
                childPath = childPath.TrimEnd(Path.DirectorySeparatorChar);
            }

            var parent = new DirectoryInfo(parentPath);
            var child = new DirectoryInfo(childPath);

            while (child.Parent != null)
            {
                if (child.Parent.FullName.Equals(parent.FullName, DiskProviderBase.PathStringComparison))
                {
                    return true;
                }

                child = child.Parent;
            }

            return false;
        }

        private static readonly Regex WindowsPathWithDriveRegex = new Regex(@"^[a-zA-Z]:\\", RegexOptions.Compiled);

        public static bool IsPathValid(this string path)
        {
            if (path.ContainsInvalidPathChars() || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (OsInfo.IsNotWindows)
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

            for (int i = 1; i < paths.Count; i++)
            {
                var path = paths[i];

                length = Math.Min(length, path.Length);

                for (int characterIndex = 0; characterIndex < length; characterIndex++)
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

        public static string ProcessNameToExe(this string processName, PlatformType runtime)
        {
            if (OsInfo.IsWindows || runtime != PlatformType.NetCore)
            {
                processName += ".exe";
            }

            return processName;
        }

        public static string ProcessNameToExe(this string processName)
        {
            return processName.ProcessNameToExe(PlatformInfo.Platform);
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

        public static string GetUpdateClientExePath(this IAppFolderInfo appFolderInfo, PlatformType runtime)
        {
            return Path.Combine(GetUpdateSandboxFolder(appFolderInfo), UPDATE_CLIENT_EXE_NAME).ProcessNameToExe(runtime);
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
    }
}
