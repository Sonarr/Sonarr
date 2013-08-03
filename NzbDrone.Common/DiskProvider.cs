using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common
{
    public interface IDiskProvider
    {
        DateTime GetLastFolderWrite(string path);
        DateTime GetLastFileWrite(string path);
        void EnsureFolder(string path);
        bool FolderExists(string path, bool caseSensitive);
        bool FolderExists(string path);
        bool FileExists(string path);
        bool FileExists(string path, bool caseSensitive);
        string[] GetDirectories(string path);
        string[] GetFiles(string path, SearchOption searchOption);
        long GetFolderSize(string path);
        long GetFileSize(string path);
        String CreateFolder(string path);
        void CopyFolder(string source, string target);
        void MoveFolder(string source, string destination);
        void DeleteFile(string path);
        void MoveFile(string source, string destination);
        void DeleteFolder(string path, bool recursive);
        void InheritFolderPermissions(string filename);
        long GetAvilableSpace(string path);
        string ReadAllText(string filePath);
        void WriteAllText(string filename, string contents);
        void FileSetLastWriteTimeUtc(string path, DateTime dateTime);
        void FolderSetLastWriteTimeUtc(string path, DateTime dateTime);
        bool IsFileLocked(FileInfo file);
        string GetPathRoot(string path);
        void SetPermissions(string filename, string account, FileSystemRights Rights, AccessControlType ControlType);
        bool IsParent(string parentfolder, string subfolder);
        FileAttributes GetFileAttributes(string path);
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

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DateTime GetLastFolderWrite(string path)
        {
            Ensure.That(() => path).IsValidPath();

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
            Ensure.That(() => path).IsValidPath();


            if (!FileExists(path))
                throw new FileNotFoundException("File doesn't exist: " + path);

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
            Ensure.That(() => path).IsValidPath();
            return Directory.Exists(path);
        }

        public bool FolderExists(string path, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return FolderExists(path) && path == path.GetActualCasing();
            }

            return FolderExists(path);
        }

        public bool FileExists(string path)
        {
            Ensure.That(() => path).IsValidPath();
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
            Ensure.That(() => path).IsValidPath();

            return Directory.GetDirectories(path);
        }

        public string[] GetFiles(string path, SearchOption searchOption)
        {
            Ensure.That(() => path).IsValidPath();

            return Directory.GetFiles(path, "*.*", searchOption);
        }

        public long GetFolderSize(string path)
        {
            Ensure.That(() => path).IsValidPath();

            return GetFiles(path, SearchOption.AllDirectories).Sum(e => new FileInfo(e).Length);
        }

        public long GetFileSize(string path)
        {
            Ensure.That(() => path).IsValidPath();

            if (!FileExists(path))
                throw new FileNotFoundException("File doesn't exist: " + path);

            var fi = new FileInfo(path);
            return fi.Length;
        }

        public String CreateFolder(string path)
        {
            Ensure.That(() => path).IsValidPath();

            return Directory.CreateDirectory(path).FullName;
        }

        public void CopyFolder(string source, string target)
        {
            Ensure.That(() => source).IsValidPath();
            Ensure.That(() => target).IsValidPath();

            TransferFolder(source, target, TransferAction.Copy);
        }

        public void MoveFolder(string source, string destination)
        {
            Ensure.That(() => source).IsValidPath();
            Ensure.That(() => destination).IsValidPath();

            try
            {
                TransferFolder(source, destination, TransferAction.Move);
                Directory.Delete(source, true);
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
            Ensure.That(() => source).IsValidPath();
            Ensure.That(() => target).IsValidPath();

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
            Ensure.That(() => path).IsValidPath();

            Logger.Trace("Deleting file: {0}", path);
            File.Delete(path);
        }

        public void MoveFile(string source, string destination)
        {
            Ensure.That(() => source).IsValidPath();
            Ensure.That(() => destination).IsValidPath();

            if (PathEquals(source, destination))
            {
                Logger.Warn("Source and destination can't be the same {0}", source);
                return;
            }

            if (FileExists(destination))
            {
                DeleteFile(destination);
            }

            File.Move(source, destination);
        }

        public void DeleteFolder(string path, bool recursive)
        {
            Ensure.That(() => path).IsValidPath();

            Directory.Delete(path, recursive);
        }

        public void InheritFolderPermissions(string filename)
        {
            Ensure.That(() => filename).IsValidPath();

            var fs = File.GetAccessControl(filename);
            fs.SetAccessRuleProtection(false, false);
            File.SetAccessControl(filename, fs);
        }

        public long GetAvilableSpace(string path)
        {
            Ensure.That(() => path).IsValidPath();

            if (!FolderExists(path))
                throw new DirectoryNotFoundException(path);


            if (OsInfo.IsLinux)
            {
                var driveInfo = DriveInfo.GetDrives().SingleOrDefault(c => c.IsReady && c.Name.Equals(Path.GetPathRoot(path), StringComparison.CurrentCultureIgnoreCase));

                if (driveInfo == null)
                {
                    return 0;
                }

                return driveInfo.AvailableFreeSpace;
            }

            return DriveFreeSpaceEx(path);
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

        public string ReadAllText(string filePath)
        {
            Ensure.That(() => filePath).IsValidPath();

            return File.ReadAllText(filePath);
        }

        public void WriteAllText(string filename, string contents)
        {
            Ensure.That(() => filename).IsValidPath();

            File.WriteAllText(filename, contents);
        }

        public static bool PathEquals(string firstPath, string secondPath)
        {
            Ensure.That(() => firstPath).IsValidPath();
            Ensure.That(() => secondPath).IsValidPath();

            return String.Equals(firstPath.CleanFilePath(), secondPath.CleanFilePath(), StringComparison.InvariantCultureIgnoreCase);
        }

        public void FileSetLastWriteTimeUtc(string path, DateTime dateTime)
        {
            Ensure.That(() => path).IsValidPath();

            File.SetLastWriteTimeUtc(path, dateTime);
        }

        public void FolderSetLastWriteTimeUtc(string path, DateTime dateTime)
        {
            Ensure.That(() => path).IsValidPath();

            Directory.SetLastWriteTimeUtc(path, dateTime);
        }

        public bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public string GetPathRoot(string path)
        {
            Ensure.That(() => path).IsValidPath();

            return Path.GetPathRoot(path);
        }

        public void SetPermissions(string filename, string account, FileSystemRights rights, AccessControlType controlType)
        {

            try
            {


                var directoryInfo = new DirectoryInfo(filename);
                var directorySecurity = directoryInfo.GetAccessControl();

                var accessRule = new FileSystemAccessRule(account, rights,
                                                          InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                          PropagationFlags.None, controlType);


                directorySecurity.AddAccessRule(accessRule);
                directoryInfo.SetAccessControl(directorySecurity);
            }
            catch (Exception e)
            {
                Logger.WarnException(string.Format("Couldn't set permission for {0}. account:{1} rights:{2} accessControlType:{3}", filename, account, rights, controlType), e);
                throw;
            }

        }

        public bool IsParent(string parent, string subfolder)
        {
            parent = parent.TrimEnd(Path.DirectorySeparatorChar);
            subfolder = subfolder.TrimEnd(Path.DirectorySeparatorChar);

            var diParent = new DirectoryInfo(parent);
            var diSubfolder = new DirectoryInfo(subfolder);

            while (diSubfolder.Parent != null)
            {
                if (diSubfolder.Parent.FullName == diParent.FullName)
                {
                    return true;
                }

                diSubfolder = diSubfolder.Parent;
            }

            return false;
        }

        public FileAttributes GetFileAttributes(string path)
        {
            return File.GetAttributes(path);
        }
    }
}