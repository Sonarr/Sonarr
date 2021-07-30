using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Unix;
using Mono.Unix.Native;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Mono.Disk
{
    public class DiskProvider : DiskProviderBase
    {
        // Mono supports sending -1 for a uint to indicate that the owner or group should not be set
        // `unchecked((uint)-1)` and `uint.MaxValue` are the same thing.
        private const uint UNCHANGED_ID = uint.MaxValue;

        private readonly Logger _logger;
        private readonly IProcMountProvider _procMountProvider;
        private readonly ISymbolicLinkResolver _symLinkResolver;
        private readonly ICreateRefLink _createRefLink;

        public DiskProvider(IProcMountProvider procMountProvider, ISymbolicLinkResolver symLinkResolver, ICreateRefLink createRefLink, Logger logger)
        {
            _procMountProvider = procMountProvider;
            _symLinkResolver = symLinkResolver;
            _createRefLink = createRefLink;
            _logger = logger;
        }

        public override IMount GetMount(string path)
        {
            path = _symLinkResolver.GetCompleteRealPath(path);

            return base.GetMount(path);
        }

        public override long? GetAvailableSpace(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var mount = GetMount(path);

            if (mount == null)
            {
                _logger.Debug("Unable to get free space for '{0}', unable to find suitable drive", path);
                return null;
            }

            return mount.AvailableFreeSpace;
        }

        public override void InheritFolderPermissions(string filename)
        {

        }

        public override void SetEveryonePermissions(string filename)
        {
        }

        public override void SetFilePermissions(string path, string mask, string group)
        {
            var permissions = NativeConvert.FromOctalPermissionString(mask);

            SetPermissions(path, mask, group, permissions);
        }

        public override void SetPermissions(string path, string mask, string group)
        {
            var permissions = NativeConvert.FromOctalPermissionString(mask);

            if (File.Exists(path))
            {
                permissions = GetFilePermissions(permissions);
            }

            SetPermissions(path, mask, group, permissions);
        }

        protected void SetPermissions(string path, string mask, string group, FilePermissions permissions)
        {
            _logger.Debug("Setting permissions: {0} on {1}", mask, path);

            // Preserve non-access permissions
            if (Syscall.stat(path, out var curStat) < 0)
            {
                var error = Stdlib.GetLastError();

                throw new LinuxPermissionsException("Error getting current permissions: " + error);
            }

            // Preserve existing non-access permissions unless mask is 4 digits
            if (mask.Length < 4)
            {
                permissions |= curStat.st_mode & ~FilePermissions.ACCESSPERMS;
            }

            if (Syscall.chmod(path, permissions) < 0)
            {
                var error = Stdlib.GetLastError();

                throw new LinuxPermissionsException("Error setting permissions: " + error);
            }

            var groupId = GetGroupId(group);

            if (Syscall.chown(path, unchecked((uint)-1), groupId) < 0)
            {
                var error = Stdlib.GetLastError();

                throw new LinuxPermissionsException("Error setting group: " + error);
            }
        }

        private static FilePermissions GetFilePermissions(FilePermissions permissions)
        {
            permissions &= ~(FilePermissions.S_IXUSR | FilePermissions.S_IXGRP | FilePermissions.S_IXOTH);

            return permissions;
        }

        public override bool IsValidFolderPermissionMask(string mask)
        {
            try
            {
                var permissions = NativeConvert.FromOctalPermissionString(mask);

                if ((permissions & ~FilePermissions.ACCESSPERMS) != 0)
                {
                    // Only allow access permissions
                    return false;
                }

                if ((permissions & FilePermissions.S_IRWXU) != FilePermissions.S_IRWXU)
                {
                    // We expect at least full owner permissions (700)
                    return false;
                }

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public override void CopyPermissions(string sourcePath, string targetPath)
        {
            try
            {
                Syscall.stat(sourcePath, out var srcStat);
                Syscall.stat(targetPath, out var tgtStat);

                if (srcStat.st_mode != tgtStat.st_mode)
                {
                    Syscall.chmod(targetPath, srcStat.st_mode);
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to copy permissions from {0} to {1}", sourcePath, targetPath);
            }
        }

        protected override List<IMount> GetAllMounts()
        {
            return _procMountProvider.GetMounts()
                                     .Concat(GetDriveInfoMounts()
                                                 .Select(d => new DriveInfoMount(d, FindDriveType.Find(d.DriveFormat)))
                                                 .Where(d => d.DriveType == DriveType.Fixed ||
                                                             d.DriveType == DriveType.Network ||
                                                             d.DriveType == DriveType.Removable))
                                     .DistinctBy(v => v.RootDirectory)
                                     .ToList();
        }

        protected override bool IsSpecialMount(IMount mount)
        {
            var root = mount.RootDirectory;

            if (root.StartsWith("/var/lib/"))
            {
                // Could be /var/lib/docker when docker uses zfs. Very unlikely that a useful mount is located in /var/lib.
                return true;
            }

            if (root.StartsWith("/snap/"))
            {
                // Mount point for snap packages
                return true;
            }

            return false;
        }

        public override long? GetTotalSize(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var mount = GetMount(path);

            return mount?.TotalSize;
        }

        protected override void CloneFileInternal(string source, string destination, bool overwrite)
        {
            if (!File.Exists(destination) && !UnixFileSystemInfo.GetFileSystemEntry(source).IsSymbolicLink)
            {
                if (_createRefLink.TryCreateRefLink(source, destination))
                {
                    return;
                }
            }

            CopyFileInternal(source, destination, overwrite);
        }

        protected override void CopyFileInternal(string source, string destination, bool overwrite)
        {
            var sourceInfo = UnixFileSystemInfo.GetFileSystemEntry(source);

            if (sourceInfo.IsSymbolicLink)
            {
                var isSameDir = UnixPath.GetDirectoryName(source) == UnixPath.GetDirectoryName(destination);
                var symlinkInfo = (UnixSymbolicLinkInfo)sourceInfo;
                var symlinkPath = symlinkInfo.ContentsPath;

                var newFile = new UnixSymbolicLinkInfo(destination);

                if (FileExists(destination) && overwrite)
                {
                    DeleteFile(destination);
                }

                if (isSameDir)
                {
                    // We're in the same dir, so we can preserve relative symlinks.
                    newFile.CreateSymbolicLinkTo(symlinkInfo.ContentsPath);
                }
                else
                {
                    var fullPath = UnixPath.Combine(UnixPath.GetDirectoryName(source), symlinkPath);
                    newFile.CreateSymbolicLinkTo(fullPath);
                }
            }
            else if (((PlatformInfo.Platform == PlatformType.Mono && PlatformInfo.GetVersion() >= new Version(6, 0)) ||
                      PlatformInfo.Platform == PlatformType.NetCore) &&
                     (!FileExists(destination) || overwrite))
            {
                TransferFilePatched(source, destination, overwrite, false);
            }
            else
            {
                base.CopyFileInternal(source, destination, overwrite);
            }
        }

        protected override void MoveFileInternal(string source, string destination)
        {
            var sourceInfo = UnixFileSystemInfo.GetFileSystemEntry(source);

            if (sourceInfo.IsSymbolicLink)
            {
                var isSameDir = UnixPath.GetDirectoryName(source) == UnixPath.GetDirectoryName(destination);
                var symlinkInfo = (UnixSymbolicLinkInfo)sourceInfo;
                var symlinkPath = symlinkInfo.ContentsPath;

                var newFile = new UnixSymbolicLinkInfo(destination);

                if (isSameDir)
                {
                    // We're in the same dir, so we can preserve relative symlinks.
                    newFile.CreateSymbolicLinkTo(symlinkInfo.ContentsPath);
                }
                else
                {
                    var fullPath = UnixPath.Combine(UnixPath.GetDirectoryName(source), symlinkPath);
                    newFile.CreateSymbolicLinkTo(fullPath);
                }

                try
                {
                    // Finally remove the original symlink.
                    symlinkInfo.Delete();
                }
                catch
                {
                    // Removing symlink failed, so rollback the new link and throw.
                    newFile.Delete();
                    throw;
                }
            }
            else if ((PlatformInfo.Platform == PlatformType.Mono && PlatformInfo.GetVersion() >= new Version(6, 0)) ||
                     PlatformInfo.Platform == PlatformType.NetCore)
            {
                TransferFilePatched(source, destination, false, true);
            }
            else
            {
                base.MoveFileInternal(source, destination);
            }
        }

        private void TransferFilePatched(string source, string destination, bool overwrite, bool move)
        {
            // Mono 6.x throws errors if permissions or timestamps cannot be set
            // - In 6.0 it'll leave a full length file
            // - In 6.6 it'll leave a zero length file
            // Catch the exception and attempt to handle these edgecases

            // Mono 6.x till 6.10 doesn't properly try use rename first.
            if (move && (PlatformInfo.Platform == PlatformType.NetCore))
            {
                if (Syscall.lstat(source, out var sourcestat) == 0 &&
                    Syscall.lstat(destination, out var deststat) != 0 &&
                    Syscall.rename(source, destination) == 0)
                {
                    _logger.Trace("Moved '{0}' -> '{1}' using Syscall.rename", source, destination);
                    return;
                }
            }

            try
            {
                if (move)
                {
                    base.MoveFileInternal(source, destination);
                }
                else
                {
                    base.CopyFileInternal(source, destination);
                }
            }
            catch (UnauthorizedAccessException)
            {
                var srcInfo = new FileInfo(source);
                var dstInfo = new FileInfo(destination);
                var exists = dstInfo.Exists && srcInfo.Exists;

                if (PlatformInfo.Platform == PlatformType.NetCore && exists && dstInfo.Length == srcInfo.Length)
                {
                    // mono 6.0, mono 6.4 and netcore 3.1 bug: full length file since utime and chmod happens at the end
                    _logger.Debug("{3} failed to {2} file likely due to known {3} bug, attempting to {2} directly. '{0}' -> '{1}'", source, destination, move ? "move" : "copy", PlatformInfo.PlatformName);

                    // Check at least part of the file since UnauthorizedAccess can happen due to legitimate reasons too
                    var checkLength = (int)Math.Min(64 * 1024, dstInfo.Length);
                    if (checkLength > 0)
                    {
                        var srcData = new byte[checkLength];
                        var dstData = new byte[checkLength];

                        _logger.Trace("Check last {0} bytes from {1}", checkLength, destination);

                        using (var srcStream = new FileStream(source, FileMode.Open, FileAccess.Read))
                        using (var dstStream = new FileStream(destination, FileMode.Open, FileAccess.Read))
                        {
                            srcStream.Position = srcInfo.Length - checkLength;
                            dstStream.Position = dstInfo.Length - checkLength;

                            srcStream.Read(srcData, 0, checkLength);
                            dstStream.Read(dstData, 0, checkLength);
                        }

                        for (var i = 0; i < checkLength; i++)
                        {
                            if (srcData[i] != dstData[i])
                            {
                                // Files aren't the same, the UnauthorizedAccess was unrelated
                                _logger.Trace("Copy was incomplete, rethrowing original error");
                                throw;
                            }
                        }

                        _logger.Trace("Copy was complete, finishing {0} operation", move ? "move" : "copy");
                    }
                }
                else
                {
                    // Unrecognized situation, the UnauthorizedAccess was unrelated
                    throw;
                }

                if (exists)
                {
                    try
                    {
                        dstInfo.LastWriteTimeUtc = srcInfo.LastWriteTimeUtc;
                    }
                    catch
                    {
                        _logger.Debug("Unable to change last modified date for {0}, skipping.", destination);
                    }

                    if (move)
                    {
                        _logger.Trace("Removing source file {0}", source);
                        File.Delete(source);
                    }
                }
            }
        }

        public override bool TryRenameFile(string source, string destination)
        {
            return Syscall.rename(source, destination) == 0;
        }

        public override bool TryCreateHardLink(string source, string destination)
        {
            try
            {
                var fileInfo = UnixFileSystemInfo.GetFileSystemEntry(source);

                if (fileInfo.IsSymbolicLink)
                {
                    return false;
                }

                fileInfo.CreateLink(destination);
                return true;
            }
            catch (UnixIOException ex)
            {
                if (ex.ErrorCode == Errno.EXDEV)
                {
                    _logger.Trace("Hardlink '{0}' to '{1}' failed due to cross-device access.", source, destination);
                }
                else
                {
                    _logger.Debug(ex, "Hardlink '{0}' to '{1}' failed.", source, destination);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Hardlink '{0}' to '{1}' failed.", source, destination);
                return false;
            }
        }

        public override bool TryCreateRefLink(string source, string destination)
        {
            return _createRefLink.TryCreateRefLink(source, destination);
        }

        private uint GetUserId(string user)
        {
            if (user.IsNullOrWhiteSpace())
            {
                return UNCHANGED_ID;
            }

            uint userId;

            if (uint.TryParse(user, out userId))
            {
                return userId;
            }

            var u = Syscall.getpwnam(user);

            if (u == null)
            {
                throw new LinuxPermissionsException("Unknown user: {0}", user);
            }

            return u.pw_uid;
        }

        private uint GetGroupId(string group)
        {
            if (group.IsNullOrWhiteSpace())
            {
                return UNCHANGED_ID;
            }

            uint groupId;

            if (uint.TryParse(group, out groupId))
            {
                return groupId;
            }

            var g = Syscall.getgrnam(group);

            if (g == null)
            {
                throw new LinuxPermissionsException("Unknown group: {0}", group);
            }

            return g.gr_gid;
        }
    }
}
