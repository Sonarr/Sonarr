using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Windows.Disk
{
    public class DiskProvider : DiskProviderBase
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(DiskProvider));

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
        out ulong lpFreeBytesAvailable,
        out ulong lpTotalNumberOfBytes,
        out ulong lpTotalNumberOfFreeBytes);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        public override IMount GetMount(string path)
        {
            var reparsePoint = GetReparsePoint(path);

            return reparsePoint ?? base.GetMount(path);
        }

        public override string GetPathRoot(string path)
        {
            Ensure.That(path, () => path).IsValidPath(PathValidationType.CurrentOs);

            var reparsePoint = GetReparsePoint(path);

            return reparsePoint?.RootDirectory ?? base.GetPathRoot(path);
        }

        public override long? GetAvailableSpace(string path)
        {
            Ensure.That(path, () => path).IsValidPath(PathValidationType.CurrentOs);

            var root = GetPathRoot(path);

            if (!FolderExists(root))
            {
                throw new DirectoryNotFoundException(root);
            }

            return DriveFreeSpaceEx(root);
        }

        public override void InheritFolderPermissions(string filename)
        {
            Ensure.That(filename, () => filename).IsValidPath(PathValidationType.CurrentOs);

            var fileInfo = new FileInfo(filename);
            var fs = fileInfo.GetAccessControl(AccessControlSections.Access);
            fs.SetAccessRuleProtection(false, false);
            fileInfo.SetAccessControl(fs);
        }

        public override void SetCurrentUserPermissions(string filename)
        {
            using var identity = WindowsIdentity.GetCurrent();

            var sid = identity.User;

            if (sid == null)
            {
                throw new InvalidOperationException($"Unable to determine the current user to set permissions for {filename}");
            }

            GrantFolderPermissions(filename, sid);
        }

        public override void SetServiceAccountPermissions(string filename)
        {
            GrantFolderPermissions(filename, new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null));
        }

        public override void RemoveEveryonePermissions(string filename)
        {
            try
            {
                var everyoneSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

                var directoryInfo = new DirectoryInfo(filename);
                var directorySecurity = directoryInfo.GetAccessControl(AccessControlSections.Access);

                var everyoneRules = directorySecurity.GetAccessRules(true, false, typeof(SecurityIdentifier))
                                                     .OfType<FileSystemAccessRule>()
                                                     .Where(acl => acl.AccessControlType == AccessControlType.Allow && acl.IdentityReference.Equals(everyoneSid))
                                                     .ToList();

                if (everyoneRules.Empty())
                {
                    return;
                }

                foreach (var everyoneRule in everyoneRules)
                {
                    directorySecurity.RemoveAccessRuleSpecific(everyoneRule);
                }

                directoryInfo.SetAccessControl(directorySecurity);
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Couldn't remove everyone permissions for {0}", filename);
                throw;
            }
        }

        public override void SetFilePermissions(string path, string mask, string group)
        {
        }

        public override void SetPermissions(string path, string mask, string group)
        {
        }

        public override void CopyPermissions(string sourcePath, string targetPath)
        {
        }

        public override long? GetTotalSize(string path)
        {
            Ensure.That(path, () => path).IsValidPath(PathValidationType.CurrentOs);

            var root = GetPathRoot(path);

            if (!FolderExists(root))
            {
                throw new DirectoryNotFoundException(root);
            }

            return DriveTotalSizeEx(root);
        }

        private static long DriveFreeSpaceEx(string folderName)
        {
            Ensure.That(folderName, () => folderName).IsValidPath(PathValidationType.CurrentOs);

            if (!folderName.EndsWith("\\"))
            {
                folderName += '\\';
            }

            if (GetDiskFreeSpaceEx(folderName, out var free, out var dummy1, out var dummy2))
            {
                return (long)free;
            }

            return 0;
        }

        private static long DriveTotalSizeEx(string folderName)
        {
            Ensure.That(folderName, () => folderName).IsValidPath(PathValidationType.CurrentOs);

            if (!folderName.EndsWith("\\"))
            {
                folderName += '\\';
            }

            if (GetDiskFreeSpaceEx(folderName, out var dummy1, out var total, out var dummy2))
            {
                return (long)total;
            }

            return 0;
        }

        public override bool TryCreateHardLink(string source, string destination)
        {
            try
            {
                if (source.Length > 256 && !source.StartsWith(@"\\?\"))
                {
                    source = @"\\?\" + source;
                }

                return CreateHardLink(destination, source, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, string.Format("Hardlink '{0}' to '{1}' failed.", source, destination));
                return false;
            }
        }

        private IMount GetReparsePoint(string path)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }

            var di = new DirectoryInfo(path);
            var isReparsePoint = di.Attributes.HasFlag(FileAttributes.ReparsePoint);

            while (!isReparsePoint && (di = di.Parent) != null)
            {
                isReparsePoint = di.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }

            if (isReparsePoint)
            {
                return new FolderMount(di);
            }

            return null;
        }

        private static void GrantFolderPermissions(string filename, SecurityIdentifier sid)
        {
            var rights = FileSystemRights.Modify;
            var controlType = AccessControlType.Allow;

            try
            {
                var directoryInfo = new DirectoryInfo(filename);
                var directorySecurity = directoryInfo.GetAccessControl(AccessControlSections.Access);

                var accessRule = new FileSystemAccessRule(sid,
                                                          rights,
                                                          InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                          PropagationFlags.None,
                                                          controlType);

                var hasSidRule = directorySecurity.GetAccessRules(true, false, typeof(SecurityIdentifier))
                                                  .OfType<FileSystemAccessRule>()
                                                  .Any(acl => acl.AccessControlType == controlType &&
                                                              (acl.FileSystemRights & rights) == rights &&
                                                              acl.InheritanceFlags == accessRule.InheritanceFlags &&
                                                              acl.PropagationFlags == accessRule.PropagationFlags &&
                                                              acl.IdentityReference.Equals(sid));

                if (hasSidRule)
                {
                    return;
                }

                directorySecurity.ModifyAccessRule(AccessControlModification.Add, accessRule, out _);

                directoryInfo.SetAccessControl(directorySecurity);
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Couldn't set permission for {0}. rights:{1} accessControlType:{2}", filename, rights, controlType);
                throw;
            }
        }
    }
}
