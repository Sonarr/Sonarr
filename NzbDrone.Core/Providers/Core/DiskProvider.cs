using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Ionic.Zip;
using NLog;

namespace NzbDrone.Core.Providers.Core
{
    public class DiskProvider
    {
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
            var fi = new FileInfo(path);
            return fi.Length;
            //return new FileInfo(path).Length;
        }

        public virtual String CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path).FullName;
        }

        public virtual void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public virtual void MoveFile(string sourcePath, string destinationPath)
        {
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
            return new DirectoryInfo(path).GetFiles(pattern, searchOption);
        }

        public virtual void MoveDirectory(string source, string destination)
        {
            Directory.Move(source, destination);
        }

        public virtual void ExtractArchive(string compressedFile, string destination)
        {
            Logger.Trace("Extracting archive [{0}] to [{1}]", compressedFile, destination);

            using (ZipFile zipFile = ZipFile.Read(compressedFile))
            {
                zipFile.ExtractAll(destination);
            }

            Logger.Trace("Extraction complete.");
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