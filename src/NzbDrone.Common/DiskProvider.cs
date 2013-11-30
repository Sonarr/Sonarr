using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common
{
    public interface IDiskProvider
    {
        DateTime GetLastFolderWrite(string path);
        DateTime GetLastFileWrite(string path);
        void EnsureFolder(string path);
        bool FolderExists(string path);
        bool FileExists(string path);
        bool FileExists(string path, bool caseSensitive);
        string[] GetDirectories(string path);
        string[] GetFiles(string path, SearchOption searchOption);
        long GetFolderSize(string path);
        long GetFileSize(string path);
        void CreateFolder(string path);
        void CopyFolder(string source, string destination);
        void MoveFolder(string source, string destination);
        void DeleteFile(string path);
        void MoveFile(string source, string destination);
        void DeleteFolder(string path, bool recursive);
        void InheritFolderPermissions(string filename);
        long? GetAvailableSpace(string path);
        string ReadAllText(string filePath);
        void WriteAllText(string filename, string contents);
        void FileSetLastWriteTimeUtc(string path, DateTime dateTime);
        void FolderSetLastWriteTimeUtc(string path, DateTime dateTime);
        bool IsFileLocked(string path);
        string GetPathRoot(string path);
        string GetParentFolder(string path);
        void SetPermissions(string filename, WellKnownSidType accountSid, FileSystemRights rights, AccessControlType controlType);
        bool IsParent(string parentPath, string childPath);
        void SetFolderWriteTime(string path, DateTime time);
        FileAttributes GetFileAttributes(string path);
        void EmptyFolder(string path);
        string[] GetFixedDrives();
        long? GetTotalSize(string path);
        string GetVolumeLabel(string path);
    }

    public class DiskProvider : IDiskProvider
    {
        enum TransferAction
        {
            Copy,
            Move
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
        out ulong lpFreeBytesAvailable,
        out ulong lpTotalNumberOfBytes,
        out ulong lpTotalNumberOfFreeBytes);

        private static readonly Logger Logger = NzbDroneLogger.GetLogger();

        public DateTime GetLastFolderWrite(string path)
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

        public DateTime GetLastFileWrite(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            if (!FileExists(path))
            {
                throw new FileNotFoundException("File doesn't exist: " + path);
            }

            return new FileInfo(path).LastWriteTimeUtc;
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

            Logger.Trace("{0} {1} -> {2}", transferAction, source, target);

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

                Logger.Trace("{0} {1} -> {2}", transferAction, sourceFile, destFile);

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

        public void InheritFolderPermissions(string filename)
        {
            Ensure.That(filename, () => filename).IsValidPath();

            try
            {
                var fs = File.GetAccessControl(filename);
                fs.SetAccessRuleProtection(false, false);
                File.SetAccessControl(filename, fs);
            }
            catch (NotImplementedException)
            {
                if (!OsInfo.IsLinux)
                {
                    throw;
                }
            }
        }

        public long? GetAvailableSpace(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var root = GetPathRoot(path);

            if (!FolderExists(root))
                throw new DirectoryNotFoundException(root);

            if (OsInfo.IsLinux)
            {
                try
                {
                    return GetDriveInfoLinux(path).AvailableFreeSpace;
                }
                catch (InvalidOperationException e)
                {
                    Logger.ErrorException("Couldn't get free space for " + path, e);
                }

                return null;
            }

            return DriveFreeSpaceEx(root);
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

        public bool IsParent(string parentPath, string childPath)
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

        public void SetFolderWriteTime(string path, DateTime time)
        {
            Directory.SetLastWriteTimeUtc(path, time);
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

        public long? GetTotalSize(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var root = GetPathRoot(path);

            if (!FolderExists(root))
                throw new DirectoryNotFoundException(root);

            if (OsInfo.IsLinux)
            {
                try
                {
                    return GetDriveInfoLinux(path).TotalSize;
                }
                catch (InvalidOperationException e)
                {
                    Logger.ErrorException("Couldn't get total space for " + path, e);
                }

                return null;
            }

            return DriveTotalSizeEx(root);
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

        private static long DriveFreeSpaceEx(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            if (!folderName.EndsWith("\\"))
            {
                folderName += '\\';
            }

            ulong free = 0;
            ulong dummy1 = 0;
            ulong dummy2 = 0;

            if (GetDiskFreeSpaceEx(folderName, out free, out dummy1, out dummy2))
            {
                return (long)free;
            }

            return 0;
        }

        private static long DriveTotalSizeEx(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            if (!folderName.EndsWith("\\"))
            {
                folderName += '\\';
            }

            ulong total = 0;
            ulong dummy1 = 0;
            ulong dummy2 = 0;

            if (GetDiskFreeSpaceEx(folderName, out dummy1, out total, out dummy2))
            {
                return (long)total;
            }

            return 0;
        }

        private DriveInfo GetDriveInfoLinux(string path)
        {
            var drives = DriveInfo.GetDrives();

            return
                drives.Where(drive =>
                    drive.IsReady && path.StartsWith(drive.Name, StringComparison.CurrentCultureIgnoreCase))
                    .OrderByDescending(drive => drive.Name.Length)
                    .First();
        }
    }
}