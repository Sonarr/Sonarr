using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.DiskTests
{
    [TestFixture]
    public class DiskTransferServiceFixture : TestBase<DiskTransferService>
    {
        private readonly string _sourcePath = @"C:\source\my.video.mkv".AsOsAgnostic();
        private readonly string _targetPath = @"C:\target\my.video.mkv".AsOsAgnostic();
        private readonly string _nfsFile = ".nfs01231232";

        private MockMount _sourceMount;
        private MockMount _targetMount;

        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IDiskProvider>(MockBehavior.Strict);

            _sourceMount = new MockMount()
            {
                Name = "source",
                RootDirectory = @"C:\source".AsOsAgnostic()
            };
            _targetMount = new MockMount()
            {
                Name = "target",
                RootDirectory = @"C:\target".AsOsAgnostic()
            };

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.GetMount(It.IsAny<string>()))
                .Returns((IMount)null);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.GetMount(It.Is<string>(p => p.StartsWith(_sourceMount.RootDirectory))))
                .Returns<string>(s => _sourceMount);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.GetMount(It.Is<string>(p => p.StartsWith(_targetMount.RootDirectory))))
                .Returns<string>(s => _targetMount);

            WithEmulatedDiskProvider();

            WithExistingFile(_sourcePath);
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
            var backupPath = _sourcePath + ".backup~";
            var targetPath = Path.Combine(Path.GetDirectoryName(_sourcePath), Path.GetFileName(_sourcePath).ToUpper());

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_sourcePath, backupPath, true))
                .Callback(() =>
                {
                    WithExistingFile(backupPath, true);
                    WithExistingFile(_sourcePath, false);
                });

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(backupPath, targetPath, false))
                .Callback(() =>
                {
                    WithExistingFile(targetPath, true);
                    WithExistingFile(backupPath, false);
                });

            var result = Subject.TransferFile(_sourcePath, targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(backupPath, targetPath, false), Times.Once());
        }

        [Test]
        public void should_rollback_rename_via_temp_on_exception()
        {
            var backupPath = _sourcePath + ".backup~";
            var targetPath = Path.Combine(Path.GetDirectoryName(_sourcePath), Path.GetFileName(_sourcePath).ToUpper());

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_sourcePath, backupPath, true))
                .Callback(() =>
                {
                    WithExistingFile(backupPath, true);
                    WithExistingFile(_sourcePath, false);
                });

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(backupPath, targetPath, false))
                .Throws(new IOException("Access Violation"));

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, targetPath, TransferMode.Move));

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(backupPath, _sourcePath, false), Times.Once());
        }

        [Test]
        public void should_log_error_if_rollback_move_fails()
        {
            var backupPath = _sourcePath + ".backup~";
            var targetPath = Path.Combine(Path.GetDirectoryName(_sourcePath), Path.GetFileName(_sourcePath).ToUpper());

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_sourcePath, backupPath, true))
                .Callback(() =>
                {
                    WithExistingFile(backupPath, true);
                    WithExistingFile(_sourcePath, false);
                });

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(backupPath, targetPath, false))
                .Throws(new IOException("Access Violation"));

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(backupPath, _sourcePath, false))
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
        public void should_use_copy_delete_on_cifs()
        {
            _sourceMount.DriveFormat = "ext4";
            _targetMount.DriveFormat = "cifs";

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_sourcePath, _targetPath, false), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.CopyFile(_sourcePath, _targetPath, false), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_sourcePath), Times.Once());
        }

        [Test]
        public void should_use_move_on_cifs_if_same_mount()
        {
            _sourceMount.DriveFormat = "cifs";
            _targetMount = _sourceMount;

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_sourcePath, _targetPath, false), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.CopyFile(_sourcePath, _targetPath, false), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_sourcePath), Times.Never());
        }

        [Test]
        public void should_not_verify_copy()
        {
            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Copy);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.CopyFile(_sourcePath, _targetPath, false), Times.Once());
        }

        [Test]
        public void should_not_verify_move()
        {
            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_sourcePath, _targetPath, false), Times.Once());
        }

        [Test]
        public void should_delete_existing_target_when_overwriting()
        {
            WithExistingFile(_targetPath);

            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move, true);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_targetPath), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_sourcePath, _targetPath, false), Times.Once());
        }

        [Test]
        public void should_throw_if_existing_target_when_not_overwriting()
        {
            WithExistingFile(_targetPath);

            Assert.Throws<DestinationAlreadyExistsException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move, false));

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(_targetPath), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_sourcePath, _targetPath, false), Times.Never());
        }

        [Test]
        public void should_verify_copy()
        {
            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Copy);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.GetFileSize(_sourcePath), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.GetFileSize(_targetPath), Times.Once());
        }

        [Test]
        public void should_rollback_copy_on_partial_and_throw()
        {
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
        public void should_verify_move()
        {
            Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.GetFileSize(_sourcePath), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.GetFileSize(_targetPath), Times.Once());
        }

        [Test]
        public void should_not_rollback_move_on_partial_and_throw_if_source_gone()
        {
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
        public void should_rollback_move_on_partial_if_source_remains()
        {
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
        [Retry(5)]
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
        public void CopyFolder_should_detect_caseinsensitive_parents()
        {
            WindowsOnly();

            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var root = new DirectoryInfo(GetTempFilePath());
            var source = new DirectoryInfo(root.FullName + "A/series");
            var destination = new DirectoryInfo(root.FullName + "a/series");

            Subject.TransferFolder(original.FullName, source.FullName, TransferMode.Copy);

            Assert.Throws<IOException>(() => Subject.TransferFolder(source.FullName, destination.FullName, TransferMode.Copy));
        }

        [Test]
        public void CopyFolder_should_detect_caseinsensitive_folder()
        {
            WindowsOnly();

            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var root = new DirectoryInfo(GetTempFilePath());
            var source = new DirectoryInfo(root.FullName + "A/series");
            var destination = new DirectoryInfo(root.FullName + "A/Series");

            Subject.TransferFolder(original.FullName, source.FullName, TransferMode.Copy);

            Assert.Throws<IOException>(() => Subject.TransferFolder(source.FullName, destination.FullName, TransferMode.Copy));
        }

        [Test]
        public void CopyFolder_should_not_copy_casesensitive_folder()
        {
            PosixOnly();

            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var root = new DirectoryInfo(GetTempFilePath());
            var source = new DirectoryInfo(root.FullName + "A/series");
            var destination = new DirectoryInfo(root.FullName + "A/Series");

            Subject.TransferFolder(original.FullName, source.FullName, TransferMode.Copy);

            // Note: Although technically possible top copy to different case, we're not allowing it
            Assert.Throws<IOException>(() => Subject.TransferFolder(source.FullName, destination.FullName, TransferMode.Copy));
        }

        [Test]
        public void CopyFolder_should_ignore_nfs_temp_file()
        {
            WithRealDiskProvider();

            var source = GetFilledTempFolder();

            File.WriteAllText(Path.Combine(source.FullName, _nfsFile), "SubFile1");

            var destination = new DirectoryInfo(GetTempFilePath());

            Subject.TransferFolder(source.FullName, destination.FullName, TransferMode.Copy);

            File.Exists(Path.Combine(destination.FullName, _nfsFile)).Should().BeFalse();
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
        public void MoveFolder_should_detect_caseinsensitive_parents()
        {
            WindowsOnly();

            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var root = new DirectoryInfo(GetTempFilePath());
            var source = new DirectoryInfo(root.FullName + "A/series");
            var destination = new DirectoryInfo(root.FullName + "a/series");

            Subject.TransferFolder(original.FullName, source.FullName, TransferMode.Copy);

            Assert.Throws<IOException>(() => Subject.TransferFolder(source.FullName, destination.FullName, TransferMode.Move));
        }

        [Test]
        public void MoveFolder_should_rename_caseinsensitive_folder()
        {
            WindowsOnly();

            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var root = new DirectoryInfo(GetTempFilePath());
            var source = new DirectoryInfo(root.FullName + "A/series");
            var destination = new DirectoryInfo(root.FullName + "A/Series");

            Subject.TransferFolder(original.FullName, source.FullName, TransferMode.Copy);

            Subject.TransferFolder(source.FullName, destination.FullName, TransferMode.Move);

            source.FullName.GetActualCasing().Should().Be(destination.FullName);
        }

        [Test]
        [Platform(Exclude = "MacOsX")]
        public void MoveFolder_should_rename_casesensitive_folder()
        {
            PosixOnly();

            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var root = new DirectoryInfo(GetTempFilePath());
            var source = new DirectoryInfo(root.FullName + "A/series");
            var destination = new DirectoryInfo(root.FullName + "A/Series");

            Subject.TransferFolder(original.FullName, source.FullName, TransferMode.Copy);

            Subject.TransferFolder(source.FullName, destination.FullName, TransferMode.Move);

            Directory.Exists(source.FullName).Should().Be(false);
            Directory.Exists(destination.FullName).Should().Be(true);
        }

        [Test]
        public void should_throw_if_destination_is_readonly()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.CopyFile(It.IsAny<string>(), It.IsAny<string>(), false))
                .Throws(new IOException("Access denied"));

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Copy));
        }

        [Test]
        public void MirrorFolder_should_remove_additional_files()
        {
            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var source = new DirectoryInfo(GetTempFilePath());
            var destination = new DirectoryInfo(GetTempFilePath());

            source.Create();
            Subject.TransferFolder(original.FullName, destination.FullName, TransferMode.Copy);

            var count = Subject.MirrorFolder(source.FullName, destination.FullName);

            count.Should().Equals(0);
            destination.GetFileSystemInfos().Should().BeEmpty();
        }

        [Test]
        public void MirrorFolder_should_not_remove_nfs_files()
        {
            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var source = new DirectoryInfo(GetTempFilePath());
            var destination = new DirectoryInfo(GetTempFilePath());

            source.Create();
            Subject.TransferFolder(original.FullName, destination.FullName, TransferMode.Copy);

            File.WriteAllText(Path.Combine(destination.FullName, _nfsFile), "SubFile1");

            var count = Subject.MirrorFolder(source.FullName, destination.FullName);

            count.Should().Equals(0);
            destination.GetFileSystemInfos().Should().HaveCount(1);
        }

        [Test]
        public void MirrorFolder_should_add_new_files()
        {
            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var source = new DirectoryInfo(GetTempFilePath());
            var destination = new DirectoryInfo(GetTempFilePath());

            Subject.TransferFolder(original.FullName, source.FullName, TransferMode.Copy);

            var count = Subject.MirrorFolder(source.FullName, destination.FullName);

            count.Should().Equals(3);
            VerifyCopyFolder(original.FullName, destination.FullName);
        }

        [Test]
        public void MirrorFolder_should_ignore_nfs_temp_file()
        {
            WithRealDiskProvider();

            var source = GetFilledTempFolder();

            File.WriteAllText(Path.Combine(source.FullName, _nfsFile), "SubFile1");

            var destination = new DirectoryInfo(GetTempFilePath());

            var count = Subject.MirrorFolder(source.FullName, destination.FullName);

            count.Should().Equals(3);

            File.Exists(Path.Combine(destination.FullName, _nfsFile)).Should().BeFalse();
        }

        [Test]
        public void MirrorFolder_should_not_touch_equivalent_files()
        {
            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var source = new DirectoryInfo(GetTempFilePath());
            var destination = new DirectoryInfo(GetTempFilePath());

            Subject.TransferFolder(original.FullName, source.FullName, TransferMode.Copy);

            Subject.TransferFolder(original.FullName, destination.FullName, TransferMode.Copy);

            var count = Subject.MirrorFolder(source.FullName, destination.FullName);

            count.Should().Equals(0);
            VerifyCopyFolder(original.FullName, destination.FullName);
        }

        [Test]
        public void MirrorFolder_should_handle_trailing_slash()
        {
            WithRealDiskProvider();

            var original = GetFilledTempFolder();
            var source = new DirectoryInfo(GetTempFilePath());
            var destination = new DirectoryInfo(GetTempFilePath());

            Subject.TransferFolder(original.FullName, source.FullName, TransferMode.Copy);

            var count = Subject.MirrorFolder(source.FullName + Path.DirectorySeparatorChar, destination.FullName);

            count.Should().Equals(3);
            VerifyCopyFolder(original.FullName, destination.FullName);
        }

        [Test]
        public void TransferFolder_should_use_movefolder_if_on_same_mount()
        {
            WithEmulatedDiskProvider();

            var src = @"C:\Base1\TestDir1".AsOsAgnostic();
            var dst = @"C:\Base1\TestDir2".AsOsAgnostic();

            WithMockMount(@"C:\Base1".AsOsAgnostic());
            WithExistingFile(@"C:\Base1\TestDir1\test.file.txt".AsOsAgnostic());

            Subject.TransferFolder(src, dst, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.MoveFolder(src, dst), Times.Once());
        }

        [Test]
        public void TransferFolder_should_not_use_movefolder_if_on_same_mount_but_target_already_exists()
        {
            WithEmulatedDiskProvider();

            var src = @"C:\Base1\TestDir1".AsOsAgnostic();
            var dst = @"C:\Base1\TestDir2".AsOsAgnostic();

            WithMockMount(@"C:\Base1".AsOsAgnostic());
            WithExistingFile(@"C:\Base1\TestDir1\test.file.txt".AsOsAgnostic());
            WithExistingFolder(dst);

            Subject.TransferFolder(src, dst, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.MoveFolder(src, dst), Times.Never());
        }

        [Test]
        public void TransferFolder_should_not_use_movefolder_if_on_different_mount()
        {
            WithEmulatedDiskProvider();

            var src = @"C:\Base1\TestDir1".AsOsAgnostic();
            var dst = @"C:\Base2\TestDir2".AsOsAgnostic();

            WithMockMount(@"C:\Base1".AsOsAgnostic());
            WithMockMount(@"C:\Base2".AsOsAgnostic());

            Subject.TransferFolder(src, dst, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.MoveFolder(src, dst), Times.Never());
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

        private void WithExistingFolder(string path, bool exists = true)
        {
            var dir = Path.GetDirectoryName(path);
            if (exists && dir.IsNotNullOrWhiteSpace())
                WithExistingFolder(dir);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.FolderExists(path))
                .Returns(exists);
        }

        private void WithExistingFile(string path, bool exists = true, int size = 1000)
        {
            var dir = Path.GetDirectoryName(path);
            if (exists && dir.IsNotNullOrWhiteSpace())
                WithExistingFolder(dir);

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


            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.FolderExists(It.IsAny<string>()))
                .Returns(false);

            Mocker.GetMock<IDiskProvider>()
               .Setup(v => v.CreateFolder(It.IsAny<string>()))
               .Callback<string>((f) =>
               {
                   WithExistingFolder(f);
               });

            Mocker.GetMock<IDiskProvider>()
               .Setup(v => v.MoveFolder(It.IsAny<string>(), It.IsAny<string>()))
               .Callback<string, string>((s, d) =>
               {
                   WithExistingFolder(s, false);
                   WithExistingFolder(d);
                   // Note: Should also deal with the files.
               });

            Mocker.GetMock<IDiskProvider>()
               .Setup(v => v.DeleteFolder(It.IsAny<string>(), It.IsAny<bool>()))
               .Callback<string, bool>((f, r) =>
               {
                   WithExistingFolder(f, false);
                   // Note: Should also deal with the files.
               });

            // Note: never returns anything.
            Mocker.GetMock<IDiskProvider>()
               .Setup(v => v.GetDirectoryInfos(It.IsAny<string>()))
               .Returns(new List<DirectoryInfo>());

            // Note: never returns anything.
            Mocker.GetMock<IDiskProvider>()
               .Setup(v => v.GetFileInfos(It.IsAny<string>()))
               .Returns(new List<FileInfo>());

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.CopyPermissions(It.IsAny<string>(), It.IsAny<string>()));
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
                .Setup(v => v.MoveFolder(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((v, r) => Directory.Move(v, r));

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.DeleteFolder(It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, bool>((v, r) => Directory.Delete(v, r));

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

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.OpenReadStream(It.IsAny<string>()))
                .Returns<string>(s => new FileStream(s, FileMode.Open, FileAccess.Read));

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.CopyPermissions(It.IsAny<string>(), It.IsAny<string>()));
        }

        private void WithMockMount(string root)
        {
            var rootDir = root;
            var mock = new Mock<IMount>();
            mock.SetupGet(v => v.RootDirectory)
                .Returns(rootDir);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.GetMount(It.Is<string>(s => s.StartsWith(rootDir))))
                .Returns(mock.Object);
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
