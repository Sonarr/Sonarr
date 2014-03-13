using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Exceptions;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.Disk
{
    public abstract class DiskProviderBase : IDiskProvider
    {
        enum TransferAction
        {
            Copy,
            Move
        }

        private static readonly Logger Logger = NzbDroneLogger.GetLogger();

        public abstract long? GetAvailableSpace(string path);
        public abstract void InheritFolderPermissions(string filename);
        public abstract void SetPermissions(string path, string mask, string user, string group);
        public abstract long? GetTotalSize(string path);

        public static string GetRelativePath(string parentPath, string childPath)
        {
            if (!IsParent(parentPath, childPath))
            {
                throw new NotParentException("{0} is not a child of {1}", childPath, parentPath);
            }

            return childPath.Substring(parentPath.Length).Trim(Path.DirectorySeparatorChar);
        }

        public static bool IsParent(string parentPath, string childPath)
        {
            parentPath = parentPath.TrimEnd(Path.DirectorySeparatorChar);
            childPath = childPath.TrimEnd(Path.DirectorySeparatorChar);

            var parent = new DirectoryInfo(parentPath);
            var child = new DirectoryInfo(childPath);

            while (child.Parent != null)
            {
                if (child.Parent.FullName == parent.FullName)
                {
                    return true;
                }

                child = child.Parent;
            }

            return false;
        }

        public DateTime FolderGetLastWrite(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            if (!FolderExists(path))
            {
                throw new DirectoryNotFoundException("Directory doesn't exist. " + path);
            }

            var dirFiles = GetFiles(path, SearchOption.AllDirectories).ToList();

            if (!dirFiles.Any())
            {
                return new DirectoryInfo(path).LastWriteTimeUtc;
            }

            return dirFiles.Select(f => new FileInfo(f))
                            .Max(c => c.LastWriteTimeUtc);
        }

        public DateTime FileGetLastWrite(string path)
        {
            PathEnsureFileExists(path);

            return new FileInfo(path).LastWriteTime;
        }

        public DateTime FileGetLastWriteUtc(string path)
        {
            PathEnsureFileExists(path);

            return new FileInfo(path).LastWriteTimeUtc;
        }

        private void PathEnsureFileExists(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            if (!FileExists(path))
            {
                throw new FileNotFoundException("File doesn't exist: " + path);
            }
        }

        public void EnsureFolder(string path)
        {
            if (!FolderExists(path))
            {
                CreateFolder(path);
            }
        }

        public bool FolderExists(string path)
        {
            Ensure.That(path, () => path).IsValidPath();
            return Directory.Exists(path);
        }

        public bool FileExists(string path)
        {
            Ensure.That(path, () => path).IsValidPath();
            return File.Exists(path);
        }

        public bool FileExists(string path, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return FileExists(path) && path == path.GetActualCasing();
            }

            return FileExists(path);
        }

        public string[] GetDirectories(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            return Directory.GetDirectories(path);
        }

        public string[] GetFiles(string path, SearchOption searchOption)
        {
            Ensure.That(path, () => path).IsValidPath();

            return Directory.GetFiles(path, "*.*", searchOption);
        }

        public long GetFolderSize(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            return GetFiles(path, SearchOption.AllDirectories).Sum(e => new FileInfo(e).Length);
        }

        public long GetFileSize(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            if (!FileExists(path))
            {
                throw new FileNotFoundException("File doesn't exist: " + path);
            }

            var fi = new FileInfo(path);
            return fi.Length;
        }

        public void CreateFolder(string path)
        {
            Ensure.That(path, () => path).IsValidPath();
            Directory.CreateDirectory(path);
        }

        public void CopyFolder(string source, string destination)
        {
            Ensure.That(source, () => source).IsValidPath();
            Ensure.That(destination, () => destination).IsValidPath();

            TransferFolder(source, destination, TransferAction.Copy);
        }

        public void MoveFolder(string source, string destination)
        {
            Ensure.That(source, () => source).IsValidPath();
            Ensure.That(destination, () => destination).IsValidPath();

            try
            {
                TransferFolder(source, destination, TransferAction.Move);
                DeleteFolder(source, true);
            }
            catch (Exception e)
            {
                e.Data.Add("Source", source);
                e.Data.Add("Destination", destination);
                throw;
            }
        }

        private void TransferFolder(string source, string target, TransferAction transferAction)
        {
            Ensure.That(source, () => source).IsValidPath();
            Ensure.That(target, () => target).IsValidPath();

            Logger.Debug("{0} {1} -> {2}", transferAction, source, target);

            var sourceFolder = new DirectoryInfo(source);
            var targetFolder = new DirectoryInfo(target);

            if (!targetFolder.Exists)
            {
                targetFolder.Create();
            }

            foreach (var subDir in sourceFolder.GetDirectories())
            {
                TransferFolder(subDir.FullName, Path.Combine(target, subDir.Name), transferAction);
            }

            foreach (var sourceFile in sourceFolder.GetFiles("*.*", SearchOption.TopDirectoryOnly))
            {
                var destFile = Path.Combine(target, sourceFile.Name);

                Logger.Debug("{0} {1} -> {2}", transferAction, sourceFile, destFile);

                switch (transferAction)
                {
                    case TransferAction.Copy:
                        {
                            sourceFile.CopyTo(destFile, true);
                            break;
                        }
                    case TransferAction.Move:
                        {
                            MoveFile(sourceFile.FullName, destFile);
                            break;
                        }
                }
            }
        }

        public void DeleteFile(string path)
        {
            Ensure.That(path, () => path).IsValidPath();
            Logger.Trace("Deleting file: {0}", path);

            RemoveReadOnly(path);

            File.Delete(path);
        }

        public void CopyFile(string source, string destination, bool overwrite = false)
        {
            Ensure.That(source, () => source).IsValidPath();
            Ensure.That(destination, () => destination).IsValidPath();

            if (source.PathEquals(destination))
            {
                Logger.Warn("Source and destination can't be the same {0}", source);
                return;
            }

            File.Copy(source, destination, overwrite);
        }

        public void MoveFile(string source, string destination)
        {
            Ensure.That(source, () => source).IsValidPath();
            Ensure.That(destination, () => destination).IsValidPath();

            if (source.PathEquals(destination))
            {
                Logger.Warn("Source and destination can't be the same {0}", source);
                return;
            }

            if (FileExists(destination))
            {
                DeleteFile(destination);
            }

            RemoveReadOnly(source);
            File.Move(source, destination);
        }

        public void DeleteFolder(string path, bool recursive)
        {
            Ensure.That(path, () => path).IsValidPath();

            Directory.Delete(path, recursive);
        }

        public string ReadAllText(string filePath)
        {
            Ensure.That(filePath, () => filePath).IsValidPath();

            return File.ReadAllText(filePath);
        }

        public void WriteAllText(string filename, string contents)
        {
            Ensure.That(filename, () => filename).IsValidPath();
            RemoveReadOnly(filename);
            File.WriteAllText(filename, contents);
        }

        public void FileSetLastWriteTimeUtc(string path, DateTime dateTime)
        {
            Ensure.That(path, () => path).IsValidPath();

            File.SetLastWriteTimeUtc(path, dateTime);
        }

        public void FolderSetLastWriteTimeUtc(string path, DateTime dateTime)
        {
            Ensure.That(path, () => path).IsValidPath();

            Directory.SetLastWriteTimeUtc(path, dateTime);
        }

        public void FileSetLastWriteTime(string path, DateTime dateTime)
        {
            Ensure.That(path, () => path).IsValidPath();

            File.SetLastWriteTime(path, dateTime);
        }
        public void FileSetLastAccessTime(string path, DateTime dateTime)
        {
            Ensure.That(path, () => path).IsValidPath();

            File.SetLastAccessTimeUtc(path, dateTime);
        }

        public void FileSetLastAccessTimeUtc(string path, DateTime dateTime)
        {
            Ensure.That(path, () => path).IsValidPath();

            File.SetLastAccessTimeUtc(path, dateTime);
        }

        public bool IsFileLocked(string file)
        {
            try
            {
                using (File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }

        public string GetPathRoot(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            return Path.GetPathRoot(path);
        }

        public string GetParentFolder(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var parent = Directory.GetParent(path);

            if (parent == null)
            {
                return null;
            }

            return parent.FullName;
        }

        public void SetPermissions(string filename, WellKnownSidType accountSid, FileSystemRights rights, AccessControlType controlType)
        {
            try
            {
                var sid = new SecurityIdentifier(accountSid, null);

                var directoryInfo = new DirectoryInfo(filename);
                var directorySecurity = directoryInfo.GetAccessControl();

                var accessRule = new FileSystemAccessRule(sid, rights,
                                                          InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                          PropagationFlags.None, controlType);

                directorySecurity.AddAccessRule(accessRule);
                directoryInfo.SetAccessControl(directorySecurity);
            }
            catch (Exception e)
            {
                Logger.WarnException(string.Format("Couldn't set permission for {0}. account:{1} rights:{2} accessControlType:{3}", filename, accountSid, rights, controlType), e);
                throw;
            }

        }

        private static void RemoveReadOnly(string path)
        {
            if (File.Exists(path))
            {
                var newAttributes = File.GetAttributes(path) & ~(FileAttributes.ReadOnly);
                File.SetAttributes(path, newAttributes);
            }
        }

        public FileAttributes GetFileAttributes(string path)
        {
            return File.GetAttributes(path);
        }

        public void EmptyFolder(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            foreach (var file in GetFiles(path, SearchOption.TopDirectoryOnly))
            {
                DeleteFile(file);
            }

            foreach (var directory in GetDirectories(path))
            {
                DeleteFolder(directory, true);
            }
        }

        public string[] GetFixedDrives()
        {
            return (DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed).Select(x => x.Name)).ToArray();
        }

        public string GetVolumeLabel(string path)
        {
            var driveInfo = DriveInfo.GetDrives().SingleOrDefault(d => d.Name == path);

            if (driveInfo == null)
            {
                return null;
            }

            return driveInfo.VolumeLabel;
        }
    }
}
