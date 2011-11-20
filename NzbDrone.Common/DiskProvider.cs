using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NLog;

namespace NzbDrone.Common
{
    public class DiskProvider
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

        public virtual bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public virtual IEnumerable<string> GetDirectories(string path)
        {
            return Directory.EnumerateDirectories(path);
        }

        public virtual IEnumerable<string> GetFiles(string path, SearchOption searchOption)
        {
            return Directory.EnumerateFiles(path, "*.*", searchOption);
        }

        public virtual long GetDirectorySize(string path)
        {
            return GetFiles(path, SearchOption.AllDirectories).Sum(e => new FileInfo(e).Length);
        }

        public virtual long GetSize(string path)
        {
            var fi = new FileInfo(path);
            return fi.Length;
            //return new FileInfo(path).Length;
        }

        public virtual String CreateDirectory(string path)
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

        public virtual void MoveFile(string sourcePath, string destinationPath)
        {
            if (FileExists(destinationPath))
            {
                DeleteFile(destinationPath);
            }

            File.Move(sourcePath, destinationPath);
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

        public virtual ulong FreeDiskSpace(DirectoryInfo directoryInfo)
        {
            ulong freeBytesAvailable;
            ulong totalNumberOfBytes;
            ulong totalNumberOfFreeBytes;

            bool success = GetDiskFreeSpaceEx(directoryInfo.FullName, out freeBytesAvailable, out totalNumberOfBytes,
                               out totalNumberOfFreeBytes);
            if (!success)
                throw new System.ComponentModel.Win32Exception();

            return freeBytesAvailable;
        }
    }
}