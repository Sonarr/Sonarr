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
                var driveInfo = GetDriveInfoLinux(path);

                if (driveInfo == null)
                {
                    Logger.Trace("Unable to get free space for '{0}', unable to find suitable drive", path);
                    return null;
                }

                return driveInfo.AvailableFreeSpace;
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

        public override void SetPermissions(string path, string mask, string user, string group)
        {
            Logger.Trace("Setting permissions: {0} on {1}", mask, path);

            var filePermissions = NativeConvert.FromOctalPermissionString(mask);

            if (Syscall.chmod(path, filePermissions) < 0)
            {
                var error = Stdlib.GetLastError();

                throw new LinuxPermissionsException("Error setting file permissions: " + error);
            }

            if (String.IsNullOrWhiteSpace(user) || String.IsNullOrWhiteSpace(group))
            {
                Logger.Trace("User or Group for chown not configured, skipping chown.");
                return;
            }

            uint userId;
            uint groupId;
            
            if (!uint.TryParse(user, out userId))
            {
                var u = Syscall.getpwnam(user);

                if (u == null)
                {
                    throw new LinuxPermissionsException("Unknown user: {0}", user);
                }

                userId = u.pw_uid;
            }

            if (!uint.TryParse(group, out groupId))
            {
                var g = Syscall.getgrnam(user);

                if (g == null)
                {
                    throw new LinuxPermissionsException("Unknown group: {0}", group);
                }

                groupId = g.gr_gid;
            }

            if (Syscall.chown(path, userId, groupId) < 0)
            {
                var error = Stdlib.GetLastError();

                throw new LinuxPermissionsException("Error setting file owner: " + error);
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
                    .FirstOrDefault();
        }
    }
}
