using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Disk
{
    public interface IFileSystemLookupService
    {
        FileSystemResult LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes);
    }

    public class FileSystemLookupService : IFileSystemLookupService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IRuntimeInfo _runtimeInfo;

        private readonly HashSet<string> _setToRemove = new HashSet<string>
                                                        {
                                                            // Windows
                                                            "boot",
                                                            "bootmgr",
                                                            "cache",
                                                            "msocache",
                                                            "recovery",
                                                            "$recycle.bin",
                                                            "recycler",
                                                            "system volume information",
                                                            "temporary internet files",
                                                            "windows",

                                                            // OS X
                                                            ".fseventd",
                                                            ".spotlight",
                                                            ".trashes",
                                                            ".vol",
                                                            "cachedmessages",
                                                            "caches",
                                                            "trash",

                                                            // QNAP
                                                            ".@__thumb",

                                                            // Synology
                                                            "@eadir",
                                                            "#recycle"
                                                        };

        public FileSystemLookupService(IDiskProvider diskProvider, IRuntimeInfo runtimeInfo)
        {
            _diskProvider = diskProvider;
            _runtimeInfo = runtimeInfo;
        }

        public FileSystemResult LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes)
        {
            if (query.IsNullOrWhiteSpace())
            {
                if (OsInfo.IsWindows)
                {
                    var result = new FileSystemResult();
                    result.Directories = GetDrives();

                    return result;
                }

                query = "/";
            }

            if (
                allowFoldersWithoutTrailingSlashes &&
                query.IsPathValid() &&
                _diskProvider.FolderExists(query))
            {
                return GetResult(query, includeFiles);
            }

            var lastSeparatorIndex = query.LastIndexOf(Path.DirectorySeparatorChar);
            var path = query.Substring(0, lastSeparatorIndex + 1);

            if (lastSeparatorIndex != -1)
            {
                return GetResult(path, includeFiles);
            }

            return new FileSystemResult();
        }

        private List<FileSystemModel> GetDrives()
        {
            return _diskProvider.GetMounts()
                                .Where(d =>
                                {
                                    // Fow Windows Services, exclude mapped network drives.
                                    if (_runtimeInfo.IsWindowsService)
                                    {
                                        return d.DriveType != DriveType.Network;
                                    }

                                    return true;
                                })
                                .Select(d => new FileSystemModel
                                {
                                    Type = FileSystemEntityType.Drive,
                                    Name = GetVolumeName(d),
                                    Path = d.RootDirectory,
                                    LastModified = null
                                })
                                .ToList();
        }

        private FileSystemResult GetResult(string path, bool includeFiles)
        {
            var result = new FileSystemResult();

            try
            {
                result.Parent = GetParent(path);
                result.Directories = GetDirectories(path);

                if (includeFiles)
                {
                    result.Files = GetFiles(path);
                }
            }
            catch (DirectoryNotFoundException)
            {
                return new FileSystemResult { Parent = GetParent(path) };
            }
            catch (ArgumentException)
            {
                return new FileSystemResult();
            }
            catch (IOException)
            {
                return new FileSystemResult { Parent = GetParent(path) };
            }
            catch (UnauthorizedAccessException)
            {
                return new FileSystemResult { Parent = GetParent(path) };
            }

            return result;
        }

        private List<FileSystemModel> GetDirectories(string path)
        {
            var directories = _diskProvider.GetDirectoryInfos(path)
                                           .OrderBy(d => d.Name)
                                           .Select(d => new FileSystemModel
                                           {
                                               Name = d.Name,
                                               Path = GetDirectoryPath(d.FullName.GetActualCasing()),
                                               LastModified = d.LastWriteTimeUtc,
                                               Type = FileSystemEntityType.Folder
                                           })
                                           .ToList();

            directories.RemoveAll(d => _setToRemove.Contains(d.Name.ToLowerInvariant()));

            return directories;
        }

        private List<FileSystemModel> GetFiles(string path)
        {
            return _diskProvider.GetFileInfos(path)
                                .OrderBy(d => d.Name)
                                .Select(d => new FileSystemModel
                                {
                                    Name = d.Name,
                                    Path = d.FullName.GetActualCasing(),
                                    LastModified = d.LastWriteTimeUtc,
                                    Extension = d.Extension,
                                    Size = d.Length,
                                    Type = FileSystemEntityType.File
                                })
                                .ToList();
        }

        private static string GetVolumeName(IMount mountInfo)
        {
            if (mountInfo.VolumeLabel.IsNullOrWhiteSpace())
            {
                return mountInfo.Name;
            }

            return $"{mountInfo.Name} ({mountInfo.VolumeLabel})";
        }

        private static string GetDirectoryPath(string path)
        {
            if (path.Last() != Path.DirectorySeparatorChar)
            {
                path += Path.DirectorySeparatorChar;
            }

            return path;
        }

        private static string GetParent(string path)
        {
            var di = new DirectoryInfo(path);

            if (di.Parent != null)
            {
                var parent = di.Parent.FullName;

                if (parent.Last() != Path.DirectorySeparatorChar)
                {
                    parent += Path.DirectorySeparatorChar;
                }

                return parent;
            }

            if (!path.Equals("/"))
            {
                return string.Empty;
            }

            return null;
        }
    }
}
