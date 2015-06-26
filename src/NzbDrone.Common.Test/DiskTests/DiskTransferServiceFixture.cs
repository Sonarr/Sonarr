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
        private readonly String _sourcePath = @"C:\source\my.video.mkv".AsOsAgnostic();
        private readonly String _targetPath = @"C:\target\my.video.mkv".AsOsAgnostic();
        private readonly String _backupPath = @"C:\source\my.video.mkv.backup~".AsOsAgnostic();
        private readonly String _tempTargetPath = @"C:\target\my.video.mkv.partial~".AsOsAgnostic();

        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IDiskProvider>(MockBehavior.Strict);

            WithEmulatedDiskProvider();

            WithExistingFile(_sourcePath);
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
        public void should_not_use_verified_transfer_on_windows()
        {
            WindowsOnly();

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.TryCreateHardLink(_sourcePath, _backupPath), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_sourcePath, _targetPath, false), Times.Once());
        }

        [Test]
        public void should_retry_if_partial_copy()
        {
            MonoOnly();

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
        public void should_retry_twice_if_partial_copy()
        {
            MonoOnly();

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
        public void should_hardlink_before_move()
        {
            MonoOnly();

            WithSuccessfulHardlink(_sourcePath, _backupPath);

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.TryCreateHardLink(_sourcePath, _backupPath), Times.Once());
        }

        [Test]
        public void should_remove_source_after_move()
        {
            MonoOnly();

            WithSuccessfulHardlink(_sourcePath, _backupPath);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_backupPath, _tempTargetPath, false))
                .Callback(() => WithExistingFile(_tempTargetPath, true));

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            VerifyDeletedFile(_sourcePath);
        }

        [Test]
        public void should_not_remove_source_if_partial_still_exists()
        {
            MonoOnly();

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
        public void should_rename_via_temp()
        {
            var targetPath = Path.Combine(Path.GetDirectoryName(_sourcePath), Path.GetFileName(_sourcePath).ToUpper());

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_sourcePath, _backupPath, false))
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
        public void should_remove_backup_if_move_throws()
        {
            MonoOnly();

            WithSuccessfulHardlink(_sourcePath, _backupPath);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.MoveFile(_backupPath, _tempTargetPath, false))
                .Throws(new IOException("Blackbox IO error"));

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move));

            VerifyDeletedFile(_backupPath);
        }

        [Test]
        public void should_remove_partial_if_move_fails()
        {
            MonoOnly();

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
        public void should_fallback_to_copy_if_hardlink_failed()
        {
            MonoOnly();

            WithFailedHardlink();

            var result = Subject.TransferFile(_sourcePath, _targetPath, TransferMode.Move);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.CopyFile(_sourcePath, _tempTargetPath, false), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.MoveFile(_tempTargetPath, _targetPath, false), Times.Once());

            VerifyDeletedFile(_sourcePath);
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

        [Test]
        public void should_throw_if_destination_is_child_of_source()
        {
            var childPath = Path.Combine(_sourcePath, "child");

            Assert.Throws<IOException>(() => Subject.TransferFile(_sourcePath, childPath, TransferMode.Move));
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
                .Setup(v => v.TryCreateHardLink(It.IsAny<String>(), It.IsAny<String>()))
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

        private void VerifyDeletedFile(String filePath)
        {
            var path = filePath;

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFile(path), Times.Once());
        }
    }
}
