using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Disk
{
    public interface IDiskTransferService
    {
        TransferMode TransferFolder(string sourcePath, string targetPath, TransferMode mode, bool verified = true);
        TransferMode TransferFile(string sourcePath, string targetPath, TransferMode mode, bool overwrite = false, bool verified = true);
    }

    public enum DiskTransferVerificationMode
    {
        None,
        VerifyOnly,
        Transactional
    }

    public class DiskTransferService : IDiskTransferService
    {
        private const int RetryCount = 2;

        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public DiskTransferVerificationMode VerificationMode { get; set; }

        public DiskTransferService(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;

            // TODO: Atm we haven't seen partial transfers on windows so we disable verified transfer.
            // (If enabled in the future, be sure to check specifically for ReFS, which doesn't support hardlinks.)
            VerificationMode = OsInfo.IsWindows ? DiskTransferVerificationMode.VerifyOnly : DiskTransferVerificationMode.Transactional;
        }

        public TransferMode TransferFolder(string sourcePath, string targetPath, TransferMode mode, bool verified = true)
        {
            Ensure.That(sourcePath, () => sourcePath).IsValidPath();
            Ensure.That(targetPath, () => targetPath).IsValidPath();

            if (VerificationMode != DiskTransferVerificationMode.Transactional)
            {
                verified = false;
            }

            if (!_diskProvider.FolderExists(targetPath))
            {
                _diskProvider.CreateFolder(targetPath);
            }

            var result = mode;

            foreach (var subDir in _diskProvider.GetDirectoryInfos(sourcePath))
            {
                result &= TransferFolder(subDir.FullName, Path.Combine(targetPath, subDir.Name), mode, verified);
            }

            foreach (var sourceFile in _diskProvider.GetFileInfos(sourcePath))
            {
                var destFile = Path.Combine(targetPath, sourceFile.Name);

                result &= TransferFile(sourceFile.FullName, destFile, mode, true, verified);
            }

            if (mode.HasFlag(TransferMode.Move))
            {
                _diskProvider.DeleteFolder(sourcePath, true);
            }

            return result;
        }

        public TransferMode TransferFile(string sourcePath, string targetPath, TransferMode mode, bool overwrite = false, bool verified = true)
        {
            Ensure.That(sourcePath, () => sourcePath).IsValidPath();
            Ensure.That(targetPath, () => targetPath).IsValidPath();

            if (VerificationMode != DiskTransferVerificationMode.Transactional)
            {
                verified = false;
            }

            _logger.Debug("{0} [{1}] > [{2}]", mode, sourcePath, targetPath);

            if (sourcePath == targetPath)
            {
                throw new IOException(string.Format("Source and destination can't be the same {0}", sourcePath));
            }

            if (sourcePath.PathEquals(targetPath, StringComparison.InvariantCultureIgnoreCase))
            {
                if (mode.HasFlag(TransferMode.HardLink) || mode.HasFlag(TransferMode.Copy))
                {
                    throw new IOException(string.Format("Source and destination can't be the same {0}", sourcePath));
                }

                if (mode.HasFlag(TransferMode.Move))
                {
                    var tempPath = sourcePath + ".backup~";

                    _diskProvider.MoveFile(sourcePath, tempPath, true);
                    try
                    {
                        ClearTargetPath(targetPath, overwrite);

                        _diskProvider.MoveFile(tempPath, targetPath);

                        return TransferMode.Move;
                    }
                    catch
                    {
                        RollbackMove(sourcePath, tempPath);
                        throw;
                    }
                }

                return TransferMode.None;
            }

            if (sourcePath.IsParentPath(targetPath))
            {
                throw new IOException(string.Format("Destination cannot be a child of the source [{0}] => [{1}]", sourcePath, targetPath));
            }

            ClearTargetPath(targetPath, overwrite);

            if (mode.HasFlag(TransferMode.HardLink))
            {
                var createdHardlink = _diskProvider.TryCreateHardLink(sourcePath, targetPath);
                if (createdHardlink)
                {
                    return TransferMode.HardLink;
                }
                if (!mode.HasFlag(TransferMode.Copy))
                {
                    throw new IOException("Hardlinking from '" + sourcePath + "' to '" + targetPath + "' failed.");
                }
            }

            if (verified)
            {
                if (mode.HasFlag(TransferMode.Copy))
                {
                    if (TryCopyFile(sourcePath, targetPath))
                    {
                        return TransferMode.Copy;
                    }
                }

                if (mode.HasFlag(TransferMode.Move))
                {
                    if (TryMoveFile(sourcePath, targetPath))
                    {
                        return TransferMode.Move;
                    }
                }

                throw new IOException(string.Format("Failed to completely transfer [{0}] to [{1}], aborting.", sourcePath, targetPath));
            }
            else if (VerificationMode == DiskTransferVerificationMode.VerifyOnly)
            {
                var originalSize = _diskProvider.GetFileSize(sourcePath);

                if (mode.HasFlag(TransferMode.Copy))
                {
                    try
                    {
                        _diskProvider.CopyFile(sourcePath, targetPath);

                        var targetSize = _diskProvider.GetFileSize(targetPath);
                        if (targetSize != originalSize)
                        {
                            throw new IOException(string.Format("File copy incomplete. [{0}] was {1} bytes long instead of {2} bytes.", targetPath, targetSize, originalSize));
                        }

                        return TransferMode.Copy;
                    }
                    catch
                    {
                        RollbackCopy(sourcePath, targetPath);
                        throw;
                    }
                }

                if (mode.HasFlag(TransferMode.Move))
                {
                    try
                    {
                        _diskProvider.MoveFile(sourcePath, targetPath);

                        var targetSize = _diskProvider.GetFileSize(targetPath);
                        if (targetSize != originalSize)
                        {
                            throw new IOException(string.Format("File copy incomplete, data loss may have occured. [{0}] was {1} bytes long instead of the expected {2}.", targetPath, targetSize, originalSize));
                        }

                        return TransferMode.Move;
                    }
                    catch
                    {
                        RollbackPartialMove(sourcePath, targetPath);
                        throw;
                    }
                }
            }
            else
            {
                if (mode.HasFlag(TransferMode.Copy))
                {
                    _diskProvider.CopyFile(sourcePath, targetPath);
                    return TransferMode.Copy;
                }

                if (mode.HasFlag(TransferMode.Move))
                {
                    _diskProvider.MoveFile(sourcePath, targetPath);
                    return TransferMode.Move;
                }
            }

            return TransferMode.None;
        }

        private void ClearTargetPath(string targetPath, bool overwrite)
        {
            if (_diskProvider.FileExists(targetPath))
            {
                if (overwrite)
                {
                    _diskProvider.DeleteFile(targetPath);
                }
                else
                {
                    throw new IOException(string.Format("Destination already exists [{0}]", targetPath));
                }
            }
        }

        private void RollbackPartialMove(string sourcePath, string targetPath)
        {
            try
            {
                _logger.Debug("Rolling back incomplete file move [{0}] to [{1}].", sourcePath, targetPath);

                WaitForIO();

                if (_diskProvider.FileExists(sourcePath))
                {
                    _diskProvider.DeleteFile(targetPath);
                }
                else
                {
                    _logger.Error("Failed to properly rollback the file move [{0}] to [{1}], incomplete file may be left in target path.", sourcePath, targetPath);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException(string.Format("Failed to properly rollback the file move [{0}] to [{1}], incomplete file may be left in target path.", sourcePath, targetPath), ex);
            }
        }

        private void RollbackMove(string sourcePath, string targetPath)
        {
            try
            {
                _logger.Debug("Rolling back file move [{0}] to [{1}].", sourcePath, targetPath);

                WaitForIO();

                _diskProvider.MoveFile(targetPath, sourcePath);
            }
            catch (Exception ex)
            {
                _logger.ErrorException(string.Format("Failed to properly rollback the file move [{0}] to [{1}], file may be left in target path.", sourcePath, targetPath), ex);
            }
        }

        private void RollbackCopy(string sourcePath, string targetPath)
        {
            try
            {
                _logger.Debug("Rolling back file copy [{0}] to [{1}].", sourcePath, targetPath);

                WaitForIO();

                if (_diskProvider.FileExists(targetPath))
                {
                    _diskProvider.DeleteFile(targetPath);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException(string.Format("Failed to properly rollback the file copy [{0}] to [{1}], file may be left in target path.", sourcePath, targetPath), ex);
            }
        }

        private void WaitForIO()
        {
            // This delay is intended to give the IO stack a bit of time to recover, this is especially required if remote NAS devices are involved.
            Thread.Sleep(3000);
        }

        private bool TryCopyFile(string sourcePath, string targetPath)
        {
            var originalSize = _diskProvider.GetFileSize(sourcePath);

            var tempTargetPath = targetPath + ".partial~";

            if (_diskProvider.FileExists(tempTargetPath))
            {
                _logger.Trace("Removing old partial.");
                _diskProvider.DeleteFile(tempTargetPath);
            }

            try
            {
                for (var i = 0; i <= RetryCount; i++)
                {
                    _diskProvider.CopyFile(sourcePath, tempTargetPath);

                    if (_diskProvider.FileExists(tempTargetPath))
                    {
                        var targetSize = _diskProvider.GetFileSize(tempTargetPath);
                        if (targetSize == originalSize)
                        {
                            _diskProvider.MoveFile(tempTargetPath, targetPath);
                            return true;
                        }
                    }

                    WaitForIO();

                    _diskProvider.DeleteFile(tempTargetPath);

                    if (i == RetryCount)
                    {
                        _logger.Error("Failed to completely transfer [{0}] to [{1}], aborting.", sourcePath, targetPath, i + 1, RetryCount);
                    }
                    else
                    {
                        _logger.Warn("Failed to completely transfer [{0}] to [{1}], retrying [{2}/{3}].", sourcePath, targetPath, i + 1, RetryCount);
                    }
                }
            }
            catch
            {
                WaitForIO();

                if (_diskProvider.FileExists(tempTargetPath))
                {
                    _diskProvider.DeleteFile(tempTargetPath);
                }

                throw;
            }

            return false;
        }

        private bool TryMoveFile(string sourcePath, string targetPath)
        {
            var originalSize = _diskProvider.GetFileSize(sourcePath);

            var backupPath = sourcePath + ".backup~";
            var tempTargetPath = targetPath + ".partial~";

            if (_diskProvider.FileExists(backupPath))
            {
                _logger.Trace("Removing old backup.");
                _diskProvider.DeleteFile(backupPath);
            }

            if (_diskProvider.FileExists(tempTargetPath))
            {
                _logger.Trace("Removing old partial.");
                _diskProvider.DeleteFile(tempTargetPath);
            }

            try
            {
                _logger.Trace("Attempting to move hardlinked backup.");
                if (_diskProvider.TryCreateHardLink(sourcePath, backupPath))
                {
                    _diskProvider.MoveFile(backupPath, tempTargetPath);

                    if (_diskProvider.FileExists(tempTargetPath))
                    {
                        var targetSize = _diskProvider.GetFileSize(tempTargetPath);

                        if (targetSize == originalSize)
                        {
                            _diskProvider.MoveFile(tempTargetPath, targetPath);
                            if (_diskProvider.FileExists(tempTargetPath))
                            {
                                throw new IOException(string.Format("Temporary file '{0}' still exists, aborting.", tempTargetPath));
                            }
                            _logger.Trace("Hardlink move succeeded, deleting source.");
                            _diskProvider.DeleteFile(sourcePath);
                            return true;
                        }
                    }

                    Thread.Sleep(5000);

                    _diskProvider.DeleteFile(tempTargetPath);
                }
            }
            finally
            {
                if (_diskProvider.FileExists(backupPath))
                {
                    _diskProvider.DeleteFile(backupPath);
                }
            }

            _logger.Trace("Hardlink move failed, reverting to copy.");
            if (TryCopyFile(sourcePath, targetPath))
            {
                _logger.Trace("Copy succeeded, deleting source.");
                _diskProvider.DeleteFile(sourcePath);
                return true;
            }

            _logger.Trace("Copy failed.");
            return false;
        }
    }
}
