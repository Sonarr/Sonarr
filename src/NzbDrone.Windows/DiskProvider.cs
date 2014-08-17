using System;
using System.IO;
using System.Runtime.InteropServices;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Windows
{
    public class DiskProvider : DiskProviderBase
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
        out ulong lpFreeBytesAvailable,
        out ulong lpTotalNumberOfBytes,
        out ulong lpTotalNumberOfFreeBytes);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        public override long? GetAvailableSpace(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var root = GetPathRoot(path);

            if (!FolderExists(root))
                throw new DirectoryNotFoundException(root);

            return DriveFreeSpaceEx(root);
        }

        public override void InheritFolderPermissions(string filename)
        {
            Ensure.That(filename, () => filename).IsValidPath();

            var fs = File.GetAccessControl(filename);
            fs.SetAccessRuleProtection(false, false);
            File.SetAccessControl(filename, fs);
        }

        public override void SetPermissions(string path, string mask, string user, string group)
        {
            
        }

        public override long? GetTotalSize(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var root = GetPathRoot(path);

            if (!FolderExists(root))
                throw new DirectoryNotFoundException(root);

            return DriveTotalSizeEx(root);
        }

        private static long DriveFreeSpaceEx(string folderName)
        {
            Ensure.That(folderName, () => folderName).IsValidPath();

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
            Ensure.That(folderName, () => folderName).IsValidPath();

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

        
        public override bool TryCreateHardLink(string source, string destination)
        {
            try
            {
                return CreateHardLink(destination, source, IntPtr.Zero);
            }
            catch
            {
                return false;
            }
        }
    }
}
