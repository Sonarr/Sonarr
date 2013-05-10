using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NLog;

namespace NzbDrone.Common
{
    public interface IDiskProvider
    {
        DateTime GetLastFolderWrite(string path);
        DateTime GetLastFileWrite(string path);
        void EnsureFolder(string path);
        bool FolderExists(string path);
        bool FileExists(string path);
        string[] GetDirectories(string path);
        string[] GetFiles(string path, SearchOption searchOption);
        long GetDirectorySize(string path);
        long GetSize(string path);
        String CreateFolder(string path);
        void CopyDirectory(string source, string target);
        void MoveDirectory(string source, string destination);
        void DeleteFile(string path);
        void MoveFile(string source, string destination);
        void DeleteFolder(string path, bool recursive);
        DateTime DirectoryDateCreated(string path);
        IEnumerable<FileInfo> GetFileInfos(string path, string pattern, SearchOption searchOption);
        void InheritFolderPermissions(string filename);
        long GetAvilableSpace(string path);
        string ReadAllText(string filePath);
        void WriteAllText(string filename, string contents);
        void FileSetLastWriteTimeUtc(string path, DateTime dateTime);
        void DirectorySetLastWriteTimeUtc(string path, DateTime dateTime);
        bool IsFolderLocked(string path);
        bool IsFileLocked(FileInfo file);
        bool IsChildOfPath(string child, string parent);
        string GetPathRoot(string path);
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

        public virtual DateTime GetLastFolderWrite(string path)
        {
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

        public virtual DateTime GetLastFileWrite(string path)
        {
            if (!FileExists(path))
                throw new FileNotFoundException("File doesn't exist: " + path);

            return new FileInfo(path).LastWriteTimeUtc;
        }


        public virtual void EnsureFolder(string path)
        {
            if (!FolderExists(path))
            {
                CreateFolder(path);
            }
        }

        public virtual bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public virtual string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public virtual string[] GetFiles(string path, SearchOption searchOption)
        {
            return Directory.GetFiles(path, "*.*", searchOption);
        }

        public virtual long GetDirectorySize(string path)
        {
            return GetFiles(path, SearchOption.AllDirectories).Sum(e => new FileInfo(e).Length);
        }

        public virtual long GetSize(string path)
        {
            if (!FileExists(path))
                throw new FileNotFoundException("File doesn't exist: " + path);

            var fi = new FileInfo(path);
            return fi.Length;
        }

        public virtual String CreateFolder(string path)
        {
            return Directory.CreateDirectory(path).FullName;
        }

        public virtual void CopyDirectory(string source, string target)
        {
            TransferDirectory(source, target, TransferAction.Copy);
        }

        public virtual void MoveDirectory(string source, string destination)
        {
            try
            {
                TransferDirectory(source, destination, TransferAction.Move);
                Directory.Delete(source, true);
            }
            catch (Exception e)
            {
                e.Data.Add("Source", source);
                e.Data.Add("Destination", destination);
                throw;
            }
        }

        private void TransferDirectory(string source, string target, TransferAction transferAction)
        {
            Logger.Trace("{0} {1} -> {2}", transferAction, source, target);

            var sourceFolder = new DirectoryInfo(source);
            var targetFolder = new DirectoryInfo(target);

            if (!targetFolder.Exists)
            {
                targetFolder.Create();
            }

            foreach (var subDir in sourceFolder.GetDirectories())
            {
                TransferDirectory(subDir.FullName, Path.Combine(target, subDir.Name), transferAction);
            }

            foreach (var sourceFile in sourceFolder.GetFiles("*.*", SearchOption.TopDirectoryOnly))
            {
                var destFile = Path.Combine(target, sourceFile.Name);

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

        public virtual void DeleteFile(string path)
        {
            Logger.Trace("Deleting file: {0}", path);
            File.Delete(path);
        }

        public virtual void MoveFile(string source, string destination)
        {
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

        public virtual void DeleteFolder(string path, bool recursive)
        {
            Directory.Delete(path, recursive);
        }

        public virtual DateTime DirectoryDateCreated(string path)
        {
            return Directory.GetCreationTime(path);
        }

        public virtual IEnumerable<FileInfo> GetFileInfos(string path, string pattern, SearchOption searchOption)
        {
            return new DirectoryInfo(path).EnumerateFiles(pattern, searchOption);
        }

        public virtual void InheritFolderPermissions(string filename)
        {
            var fs = File.GetAccessControl(filename);
            fs.SetAccessRuleProtection(false, false);
            File.SetAccessControl(filename, fs);
        }

        public virtual long GetAvilableSpace(string path)
        {
            if (!FolderExists(path))
                throw new DirectoryNotFoundException(path);


            var driveInfo = DriveInfo.GetDrives().SingleOrDefault(c => c.IsReady && c.Name.Equals(Path.GetPathRoot(path), StringComparison.CurrentCultureIgnoreCase));

            if (driveInfo == null)
            {
                if (EnvironmentProvider.IsLinux)
                {
                    return 0;
                }

                return DriveFreeSpaceEx(path);
            }

            return driveInfo.AvailableFreeSpace;
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

        public virtual string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public virtual void WriteAllText(string filename, string contents)
        {
            File.WriteAllText(filename, contents);
        }

        public static bool PathEquals(string firstPath, string secondPath)
        {
            return String.Equals(firstPath.CleanPath(), secondPath.CleanPath(), StringComparison.InvariantCultureIgnoreCase);
        }

        public virtual void FileSetLastWriteTimeUtc(string path, DateTime dateTime)
        {
            File.SetLastWriteTimeUtc(path, dateTime);
        }

        public virtual void DirectorySetLastWriteTimeUtc(string path, DateTime dateTime)
        {
            Directory.SetLastWriteTimeUtc(path, dateTime);
        }

        public virtual bool IsFolderLocked(string path)
        {
            var files = GetFileInfos(path, "*.*", SearchOption.AllDirectories);

            foreach (var fileInfo in files)
            {
                if (IsFileLocked(fileInfo))
                    return true;
            }

            return false;
        }

        public virtual bool IsFileLocked(FileInfo file)
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

        public virtual bool IsChildOfPath(string child, string parent)
        {
            if (Path.GetFullPath(child).StartsWith(Path.GetFullPath(parent)))
                return true;

            return false;
        }

        public virtual string GetPathRoot(string path)
        {
            return Path.GetPathRoot(path);
        }
    }
}