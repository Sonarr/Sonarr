using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Test.Common;
using FluentAssertions;

namespace NzbDrone.Common.Test.DiskTests
{
    [TestFixture]
    public class DiskTransferServiceFixture : TestBase<DiskTransferService>
    {
        private readonly string _sourcePath = @"C:\source\my.video.mkv".AsOsAgnostic();
        private readonly string _targetPath = @"C:\target\my.video.mkv".AsOsAgnostic();
        private readonly string _backupPath = @"C:\source\my.video.mkv.backup~".AsOsAgnostic();
        private readonly string _tempTargetPath = @"C:\target\my.video.mkv.partial~".AsOsAgnostic();

        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IDiskProvider>(MockBehavior.Strict);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.GetMount(It.IsAny<string>()))
                .Returns((IMount)null);

            WithEmulatedDiskProvider();

            WithExistingFile(_sourcePath);
        }

        [Test]
        public void should_use_verified_transfer_on_mono()
        {
            MonoOnly();

            Subject.VerificationMode.Should().Be(DiskTransferVerificationMode.TryTransactional);
        }

        [Test]
        public void should_not_use_verified_transfer_on_windows()
        {
            WindowsOnly();

            Subject.VerificationMode.Should().Be(DiskTransferVerificationMode.VerifyOnly);

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.TryCreateHardLink(_sourcePath, _backupPath), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_sourcePath, _targetPath, false), Times.Once());
        }

        [Test]
        public void should_throw_if_path_is_the_same()
        {
            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _sourcePath, TransferMode.HardLink));

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.TryCreateHardLink(_sourcePath, _sourcePath), Times.Never());
        }

        [Test]
        public void should_throw_if_different_casing_unless_moving()
        {
            var targetPath = Path.Combine(Path.GetDirectoryName(_sourcePath), Path.GetFileName(_sourcePath).ToUpper());

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, targetPath, TransferMode.HardLink));
        }

        [Test]
        public void should_rename_via_temp_if_different_casing()
        {
            var targetPath = Path.Combine(Path.GetDirectoryName(_sourcePath), Path.GetFileName(_sourcePath).ToUpper());

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_sourcePath, _backupPath, true))
                .Callback(() =>
                {
                    WithExistingFile(_backupPath, true);
                    WithExistingFile(_sourcePath, false);
                });

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_backupPath, targetPath, false))
                .Callback(() =>
                {
                    WithExistingFile(targetPath, true);
                    WithExistingFile(_backupPath, false);
                });

            var result = Subject.TransferFile(_sourcePath, targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_backupPath, targetPath, false), Times.Once());
        }

        [Test]
        public void should_rollback_rename_via_temp_on_exception()
        {
            var targetPath = Path.Combine(Path.GetDirectoryName(_sourcePath), Path.GetFileName(_sourcePath).ToUpper());

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_sourcePath, _backupPath, true))
                .Callback(() =>
                {
                    WithExistingFile(_backupPath, true);
                    WithExistingFile(_sourcePath, false);
                });

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_backupPath, targetPath, false))
                .Throws(new IOException("Access Violation"));

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, targetPath, TransferMode.Move));

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_backupPath, _sourcePath, false), Times.Once());
        }

        [Test]
        public void should_log_error_if_rollback_move_fails()
        {
            var targetPath = Path.Combine(Path.GetDirectoryName(_sourcePath), Path.GetFileName(_sourcePath).ToUpper());

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_sourcePath, _backupPath, true))
                .Callback(() =>
                {
                    WithExistingFile(_backupPath, true);
                    WithExistingFile(_sourcePath, false);
                });

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_backupPath, targetPath, false))
                .Throws(new IOException("Access Violation"));

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_backupPath, _sourcePath, false))
                .Throws(new IOException("Access Violation"));

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, targetPath, TransferMode.Move));

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_throw_if_destination_is_child_of_source()
        {
            var childPath = Path.Combine(_sourcePath, "child");

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, childPath, TransferMode.Move));
        }

        [Test]
        public void should_hardlink_only()
        {
            WithSuccessfulHardlink(_sourcePath, _targetPath);

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.HardLink);

            result.Should().Be(TransferMode.HardLink);
        }

        [Test]
        public void should_throw_if_hardlink_only_failed()
        {
            WithFailedHardlink();

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.HardLink));
        }

        [Test]
        public void should_fallback_to_copy_if_hardlink_failed()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            WithFailedHardlink();

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.CopyFile(_sourcePath, _tempTargetPath, false), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_tempTargetPath, _targetPath, false), Times.Once());

            VerifyDeletedFile(_sourcePath);
        }

        [Test]
        public void mode_none_should_not_verify_copy()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.None;

            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Copy);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.CopyFile(_sourcePath, _targetPath, false), Times.Once());
        }

        [Test]
        public void mode_none_should_not_verify_move()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.None;

            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_sourcePath, _targetPath, false), Times.Once());
        }

        [Test]
        public void mode_none_should_delete_existing_target_when_overwriting()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.None;

            WithExistingFile(_targetPath);

            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move, true);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_targetPath), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_sourcePath, _targetPath, false), Times.Once());
        }

        [Test]
        public void mode_none_should_throw_if_existing_target_when_not_overwriting()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.None;

            WithExistingFile(_targetPath);

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move, false));

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_targetPath), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_sourcePath, _targetPath, false), Times.Never());
        }

        [Test]
        public void mode_verifyonly_should_verify_copy()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.VerifyOnly;

            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Copy);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.GetFileSize(_sourcePath), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.GetFileSize(_targetPath), Times.Once());
        }

        [Test]
        public void mode_verifyonly_should_rollback_copy_on_partial_and_throw()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.VerifyOnly;

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.CopyFile(_sourcePath, _targetPath, false))
                .Callback(() =>
                {
                    WithExistingFile(_targetPath, true, 900);
                });

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Copy));

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_targetPath), Times.Once());
        }

        [Test]
        public void should_log_error_if_rollback_copy_fails()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.VerifyOnly;

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.CopyFile(_sourcePath, _targetPath, false))
                .Callback(() =>
                {
                    WithExistingFile(_targetPath, true, 900);
                });

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.DeleteFile(_targetPath))
                .Throws(new IOException("Access Violation"));

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Copy));

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void mode_verifyonly_should_verify_move()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.VerifyOnly;

            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);
            
            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.GetFileSize(_sourcePath), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.GetFileSize(_targetPath), Times.Once());
        }

        [Test]
        public void mode_verifyonly_should_not_rollback_move_on_partial_and_throw()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.VerifyOnly;

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_sourcePath, _targetPath, false))
                .Callback(() =>
                {
                    WithExistingFile(_sourcePath, false);
                    WithExistingFile(_targetPath, true, 900);
                });

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move));

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_targetPath), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void mode_verifyonly_should_rollback_move_on_partial_if_source_remains()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.VerifyOnly;

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_sourcePath, _targetPath, false))
                .Callback(() =>
                {
                    WithExistingFile(_targetPath, true, 900);
                });

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move));

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_targetPath), Times.Once());
        }

        [Test]
        public void should_log_error_if_rollback_partialmove_fails()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.VerifyOnly;

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_sourcePath, _targetPath, false))
                .Callback(() =>
                {
                    WithExistingFile(_targetPath, true, 900);
                });

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.DeleteFile(_targetPath))
                .Throws(new IOException("Access Violation"));

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move));

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void mode_transactional_should_move_and_verify_if_same_folder()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            var targetPath = _sourcePath + ".test";

            var result = Subject.TransferFile(_sourcePath, targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.TryCreateHardLink(_sourcePath, _backupPath), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_sourcePath, targetPath, false), Times.Once());
        }

        [Test]
        public void mode_trytransactional_should_revert_to_verifyonly_if_hardlink_fails()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.TryTransactional;

            WithFailedHardlink();

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_sourcePath, _targetPath, false))
                .Callback(() =>
                {
                    WithExistingFile(_sourcePath, false);
                    WithExistingFile(_targetPath, true);
                });

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.TryCreateHardLink(_sourcePath, _backupPath), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_sourcePath, _targetPath, false), Times.Once());
        }

        [Test]
        public void mode_transactional_should_delete_old_backup_on_move()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            WithExistingFile(_backupPath);

            WithSuccessfulHardlink(_sourcePath, _backupPath);

            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_backupPath), Times.Once());
        }

        [Test]
        public void mode_transactional_should_delete_old_partial_on_move()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            WithExistingFile(_tempTargetPath);

            WithSuccessfulHardlink(_sourcePath, _backupPath);

            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_tempTargetPath), Times.Once());
        }

        [Test]
        public void mode_transactional_should_delete_old_partial_on_copy()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            WithExistingFile(_tempTargetPath);

            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Copy);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_tempTargetPath), Times.Once());
        }

        [Test]
        public void mode_transactional_should_hardlink_before_move()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            WithSuccessfulHardlink(_sourcePath, _backupPath);

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.TryCreateHardLink(_sourcePath, _backupPath), Times.Once());
        }

        [Test]
        public void mode_transactional_should_retry_if_partial_copy()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            var retry = 0;
            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.CopyFile(_sourcePath, _tempTargetPath, false))
                .Callback(() =>
                    {
                        WithExistingFile(_tempTargetPath, true, 900);
                        if (retry++ == 1) WithExistingFile(_tempTargetPath, true, 1000);
                    });

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Copy);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void mode_transactional_should_retry_twice_if_partial_copy()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            var retry = 0;
            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.CopyFile(_sourcePath, _tempTargetPath, false))
                .Callback(() =>
                    {
                        WithExistingFile(_tempTargetPath, true, 900);
                        if (retry++ == 3) throw new Exception("Test Failed, retried too many times.");
                    });

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Copy));

            ExceptionVerification.ExpectedWarns(2);
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void mode_transactional_should_remove_source_after_move()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            WithSuccessfulHardlink(_sourcePath, _backupPath);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_backupPath, _tempTargetPath, false))
                .Callback(() => WithExistingFile(_tempTargetPath, true));

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            VerifyDeletedFile(_sourcePath);
        }

        [Test]
        public void mode_transactional_should_not_remove_source_if_partial_still_exists()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            var targetPath = Path.Combine(Path.GetDirectoryName(_targetPath), Path.GetFileName(_targetPath).ToUpper());
            var tempTargetPath = targetPath + ".partial~";

            WithSuccessfulHardlink(_sourcePath, _backupPath);

            WithExistingFile(_targetPath);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_backupPath, tempTargetPath, false))
                .Callback(() => WithExistingFile(tempTargetPath, true));

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(tempTargetPath, targetPath, false))
                .Callback(() => { });

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, targetPath, TransferMode.Move));

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_sourcePath), Times.Never());
        }

        [Test]
        public void mode_transactional_should_remove_partial_if_copy_fails()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            WithSuccessfulHardlink(_sourcePath, _backupPath);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.CopyFile(_sourcePath, _tempTargetPath, false))
                .Callback(() =>
                {
                    WithExistingFile(_tempTargetPath, true, 900);
                })
                .Throws(new IOException("Blackbox IO error"));

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Copy));

            VerifyDeletedFile(_tempTargetPath);
        }

        [Test]
        public void mode_transactional_should_remove_backup_if_move_throws()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            WithSuccessfulHardlink(_sourcePath, _backupPath);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_backupPath, _tempTargetPath, false))
                .Throws(new IOException("Blackbox IO error"));

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move));

            VerifyDeletedFile(_backupPath);
        }

        [Test]
        public void mode_transactional_should_remove_partial_if_move_fails()
        {
            Subject.VerificationMode = DiskTransferVerificationMode.Transactional;

            WithSuccessfulHardlink(_sourcePath, _backupPath);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_backupPath, _tempTargetPath, false))
                .Callback(() =>
                {
                    WithExistingFile(_backupPath, false);
                    WithExistingFile(_tempTargetPath, true, 900);
                });

            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            VerifyDeletedFile(_tempTargetPath);
        }

        [Test]
        public void CopyFolder_should_copy_folder()
        {
            WithRealDiskProvider();

            var source = GetFilledTempFolder();
            var destination = new DirectoryInfo(GetTempFilePath());

            Subject.TransferFolder(source.FullName, destination.FullName, TransferMode.Copy);

            VerifyCopyFolder(source.FullName, destination.FullName);
        }

        [Test]
        public void CopyFolder_should_overwrite_existing_folder()
        {
            WithRealDiskProvider();

            var source = GetFilledTempFolder();
            var destination = new DirectoryInfo(GetTempFilePath());
            Subject.TransferFolder(source.FullName, destination.FullName, TransferMode.Copy);

            //Delete Random File
            destination.GetFiles("*.*", SearchOption.AllDirectories).First().Delete();

            Subject.TransferFolder(source.FullName, destination.FullName, TransferMode.Copy);

            VerifyCopyFolder(source.FullName, destination.FullName);
        }


        [Test]
        public void MoveFolder_should_move_folder()
        {
            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var source = new DirectoryInfo(GetTempFilePath());
            var destination = new DirectoryInfo(GetTempFilePath());

            Subject.TransferFolder(original.FullName, source.FullName, TransferMode.Copy);

            Subject.TransferFolder(source.FullName, destination.FullName, TransferMode.Move);

            VerifyMoveFolder(original.FullName, source.FullName, destination.FullName);
        }

        [Test]
        public void MoveFolder_should_overwrite_existing_folder()
        {
            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var source = new DirectoryInfo(GetTempFilePath());
            var destination = new DirectoryInfo(GetTempFilePath());

            Subject.TransferFolder(original.FullName, source.FullName, TransferMode.Copy);
            Subject.TransferFolder(original.FullName, destination.FullName, TransferMode.Copy);

            Subject.TransferFolder(source.FullName, destination.FullName, TransferMode.Move);

            VerifyMoveFolder(original.FullName, source.FullName, destination.FullName);
        }

        [Test]
        public void should_throw_if_destination_is_readonly()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.CopyFile(It.IsAny<string>(), It.IsAny<string>(), false))
                .Throws(new IOException("Access denied"));

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Copy));
        }

        public DirectoryInfo GetFilledTempFolder()
        {
            var tempFolder = GetTempFilePath();
            Directory.CreateDirectory(tempFolder);

            File.WriteAllText(Path.Combine(tempFolder, Path.GetRandomFileName()), "RootFile");

            var subDir = Path.Combine(tempFolder, Path.GetRandomFileName());
            Directory.CreateDirectory(subDir);

            File.WriteAllText(Path.Combine(subDir, Path.GetRandomFileName()), "SubFile1");
            File.WriteAllText(Path.Combine(subDir, Path.GetRandomFileName()), "SubFile2");

            return new DirectoryInfo(tempFolder);
        }

        private void WithExistingFile(string path, bool exists = true, int size = 1000)
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.FileExists(path))
                .Returns(exists);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.GetFileSize(path))
                .Returns(size);
        }

        private void WithSuccessfulHardlink(string source, string target)
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.TryCreateHardLink(source, target))
                .Callback(() => WithExistingFile(target))
                .Returns(true);
        }

        private void WithFailedHardlink()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.TryCreateHardLink(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);
        }

        private void WithEmulatedDiskProvider()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.FileExists(It.IsAny<string>()))
                .Returns(false);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.CopyFile(It.IsAny<string>(), It.IsAny<string>(), false))
                .Callback<string, string, bool>((s, d, o) =>
                {
                    WithExistingFile(d);
                });

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(It.IsAny<string>(), It.IsAny<string>(), false))
                .Callback<string, string, bool>((s, d, o) =>
                {
                    WithExistingFile(s, false);
                    WithExistingFile(d);
                });

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.DeleteFile(It.IsAny<string>()))
                .Callback<string>(v =>
                {
                    WithExistingFile(v, false);
                });
        }

        private void WithRealDiskProvider()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.FolderExists(It.IsAny<string>()))
                .Returns<string>(v => Directory.Exists(v));

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.FileExists(It.IsAny<string>()))
                .Returns<string>(v => File.Exists(v));

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.CreateFolder(It.IsAny<string>()))
                .Callback<string>(v => Directory.CreateDirectory(v));

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.DeleteFolder(It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, bool>((v,r) => Directory.Delete(v, r));

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.DeleteFile(It.IsAny<string>()))
                .Callback<string>(v => File.Delete(v));

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.GetDirectoryInfos(It.IsAny<string>()))
                .Returns<string>(v => new DirectoryInfo(v).GetDirectories().ToList());

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.GetFileInfos(It.IsAny<string>()))
                .Returns<string>(v => new DirectoryInfo(v).GetFiles().ToList());

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.GetFileSize(It.IsAny<string>()))
                .Returns<string>(v => new FileInfo(v).Length);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.TryCreateHardLink(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, string, bool>((s, d, o) => File.Copy(s, d, o));

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, string, bool>((s,d,o) => {
                    if (File.Exists(d) && o) File.Delete(d);
                    File.Move(s, d);
                });
            
        }

        private void VerifyCopyFolder(string source, string destination)
        {
            var sourceFiles = Directory.GetFileSystemEntries(source, "*", SearchOption.AllDirectories).Select(v => v.Substring(source.Length + 1)).ToArray();
            var destFiles = Directory.GetFileSystemEntries(destination, "*", SearchOption.AllDirectories).Select(v => v.Substring(destination.Length + 1)).ToArray();

            CollectionAssert.AreEquivalent(sourceFiles, destFiles);
        }

        private void VerifyMoveFolder(string source, string from, string destination)
        {
            Directory.Exists(from).Should().BeFalse();

            var sourceFiles = Directory.GetFileSystemEntries(source, "*", SearchOption.AllDirectories).Select(v => v.Substring(source.Length + 1)).ToArray();
            var destFiles = Directory.GetFileSystemEntries(destination, "*", SearchOption.AllDirectories).Select(v => v.Substring(destination.Length + 1)).ToArray();

            CollectionAssert.AreEquivalent(sourceFiles, destFiles);
        }

        private void VerifyDeletedFile(string filePath)
        {
            var path = filePath;

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(path), Times.Once());
        }
    }
}
