using System;
using System.IO;
using System.Linq;
using Mono.Unix.Native;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Mono
{
    public class DiskProvider : DiskProviderBase
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger();

        public override long? GetAvailableSpace(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var root = GetPathRoot(path);

            if (!FolderExists(root))
                throw new DirectoryNotFoundException(root);

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

        public override void InheritFolderPermissions(string filename)
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
            }
        }

        public override void SetFilePermissions(string path, string mask)
        {
            var filePermissions = NativeConvert.FromOctalPermissionString(mask);

            if (Syscall.chmod(path, filePermissions) < 0)
            {
                var error = Stdlib.GetLastError();

                throw new Exception("Error setting file permissions: " + error);
            }
        }

        public override long? GetTotalSize(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var root = GetPathRoot(path);

            if (!FolderExists(root))
                throw new DirectoryNotFoundException(root);

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
