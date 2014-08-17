using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.DiskProviderTests
{
    public class DiskProviderFixtureBase<TSubject> : TestBase<TSubject> where TSubject : class, IDiskProvider
    {
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

        [Test]
        public void directory_exist_should_be_able_to_find_existing_folder()
        {
            Subject.FolderExists(TempFolder).Should().BeTrue();
        }

        [Test]
        public void directory_exist_should_be_able_to_find_existing_unc_share()
        {
            WindowsOnly();

            Subject.FolderExists(@"\\localhost\c$").Should().BeTrue();
        }

        [Test]
        public void directory_exist_should_not_be_able_to_find_none_existing_folder()
        {
            Subject.FolderExists(@"C:\ThisBetterNotExist\".AsOsAgnostic()).Should().BeFalse();
        }

        [Test]
        public void MoveFile_should_overwrite_existing_file()
        {
            var source1 = GetTempFilePath();
            var source2 = GetTempFilePath();
            var destination = GetTempFilePath();

            File.WriteAllText(source1, "SourceFile1");
            File.WriteAllText(source2, "SourceFile2");

            Subject.MoveFile(source1, destination);
            Subject.MoveFile(source2, destination, true);

            File.Exists(destination).Should().BeTrue();
        }

        [Test]
        public void MoveFile_should_not_move_overwrite_itself()
        {
            var source = GetTempFilePath();

            File.WriteAllText(source, "SourceFile1");

            Subject.MoveFile(source, source, true);

            File.Exists(source).Should().BeTrue();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void CopyFolder_should_copy_folder()
        {
            var source = GetFilledTempFolder();
            var destination = new DirectoryInfo(GetTempFilePath());

            Subject.CopyFolder(source.FullName, destination.FullName);

            VerifyCopy(source.FullName, destination.FullName);
        }

        [Test]
        public void CopyFolder_should_overwrite_existing_folder()
        {
            var source = GetFilledTempFolder();
            var destination = new DirectoryInfo(GetTempFilePath());
            Subject.CopyFolder(source.FullName, destination.FullName);

            //Delete Random File
            destination.GetFiles("*.*", SearchOption.AllDirectories).First().Delete();

            Subject.CopyFolder(source.FullName, destination.FullName);

            VerifyCopy(source.FullName, destination.FullName);
        }

        [Test]
        public void MoveFolder_should_move_folder()
        {
            var original = GetFilledTempFolder();
            var source = new DirectoryInfo(GetTempFilePath());
            var destination = new DirectoryInfo(GetTempFilePath());

            Subject.CopyFolder(original.FullName, source.FullName);

            Subject.MoveFolder(source.FullName, destination.FullName);

            VerifyMove(original.FullName, source.FullName, destination.FullName);
        }

        [Test]
        public void MoveFolder_should_overwrite_existing_folder()
        {
            var original = GetFilledTempFolder();
            var source = new DirectoryInfo(GetTempFilePath());
            var destination = new DirectoryInfo(GetTempFilePath());

            Subject.CopyFolder(original.FullName, source.FullName);
            Subject.CopyFolder(original.FullName, destination.FullName);

            Subject.MoveFolder(source.FullName, destination.FullName);

            VerifyMove(original.FullName, source.FullName, destination.FullName);
        }

        [Test]
        public void should_be_able_to_move_read_only_file()
        {
            var source = GetTempFilePath();
            var destination = GetTempFilePath();

            Subject.WriteAllText(source, "SourceFile");
            Subject.WriteAllText(destination, "DestinationFile");

            File.SetAttributes(source, FileAttributes.ReadOnly);
            File.SetAttributes(destination, FileAttributes.ReadOnly);

            Subject.MoveFile(source, destination, true);
        }

        [Test]
        public void should_be_able_to_delete_directory_with_read_only_file()
        {
            var sourceDir = GetTempFilePath();
            var source = Path.Combine(sourceDir, "test.txt");

            Directory.CreateDirectory(sourceDir);

            Subject.WriteAllText(source, "SourceFile");

            File.SetAttributes(source, FileAttributes.ReadOnly);

            Subject.DeleteFolder(sourceDir, true);

            Directory.Exists(sourceDir).Should().BeFalse();
        }

        [Test]
        public void should_be_able_to_hardlink_file()
        {
            var sourceDir = GetTempFilePath();
            var source = Path.Combine(sourceDir, "test.txt");
            var destination = Path.Combine(sourceDir, "destination.txt");

            Directory.CreateDirectory(sourceDir);

            Subject.WriteAllText(source, "SourceFile");

            var result = Subject.TransferFile(source, destination, TransferMode.HardLink);

            result.Should().Be(TransferMode.HardLink);

            File.AppendAllText(source, "Test");
            File.ReadAllText(destination).Should().Be("SourceFileTest");
        }

        private void DoHardLinkRename(FileShare fileShare)
        {
            var sourceDir = GetTempFilePath();
            var source = Path.Combine(sourceDir, "test.txt");
            var destination = Path.Combine(sourceDir, "destination.txt");
            var rename = Path.Combine(sourceDir, "rename.txt");

            Directory.CreateDirectory(sourceDir);

            Subject.WriteAllText(source, "SourceFile");

            Subject.TransferFile(source, destination, TransferMode.HardLink);

            using (var stream = new FileStream(source, FileMode.Open, FileAccess.Read, fileShare))
            {
                stream.ReadByte();

                Subject.MoveFile(destination, rename);

                stream.ReadByte();
            }

            File.Exists(rename).Should().BeTrue();
            File.Exists(destination).Should().BeFalse();

            File.AppendAllText(source, "Test");
            File.ReadAllText(rename).Should().Be("SourceFileTest");
        }

        [Test]
        public void should_be_able_to_rename_open_hardlinks_with_fileshare_delete()
        {
            DoHardLinkRename(FileShare.Delete);
        }

        [Test]
        public void should_not_be_able_to_rename_open_hardlinks_with_fileshare_none()
        {
            Assert.Throws<IOException>(() => DoHardLinkRename(FileShare.None));
        }

        [Test]
        public void should_not_be_able_to_rename_open_hardlinks_with_fileshare_write()
        {
            Assert.Throws<IOException>(() => DoHardLinkRename(FileShare.Read));
        }

        [Test]
        public void empty_folder_should_return_folder_modified_date()
        {
            var tempfolder = new DirectoryInfo(TempFolder);
            Subject.FolderGetLastWrite(TempFolder).Should().Be(tempfolder.LastWriteTimeUtc);
        }

        [Test]
        public void folder_should_return_correct_value_for_last_write()
        {
            var testDir = GetTempFilePath();
            var testFile = Path.Combine(testDir, Path.GetRandomFileName());

            Directory.CreateDirectory(testDir);

            Subject.FolderSetLastWriteTime(TempFolder, DateTime.UtcNow.AddMinutes(-5));

            TestLogger.Info("Path is: {0}", testFile);

            Subject.WriteAllText(testFile, "Test");

            Subject.FolderGetLastWrite(TempFolder).Should().BeOnOrAfter(DateTime.UtcNow.AddMinutes(-1));
            Subject.FolderGetLastWrite(TempFolder).Should().BeBefore(DateTime.UtcNow.AddMinutes(1));
        }

        [Test]
        public void should_return_false_for_unlocked_file()
        {
            var testFile = GetTempFilePath();
            Subject.WriteAllText(testFile, new Guid().ToString());

            Subject.IsFileLocked(testFile).Should().BeFalse();
        }

        [Test]
        public void should_return_false_for_unlocked_and_readonly_file()
        {
            var testFile = GetTempFilePath();
            Subject.WriteAllText(testFile, new Guid().ToString());

            File.SetAttributes(testFile, FileAttributes.ReadOnly);

            Subject.IsFileLocked(testFile).Should().BeFalse();
        }

        [Test]
        public void should_return_true_for_unlocked_file()
        {
            var testFile = GetTempFilePath();
            Subject.WriteAllText(testFile, new Guid().ToString());

            using (var file = File.OpenWrite(testFile))
            {
                Subject.IsFileLocked(testFile).Should().BeTrue();
            }
        }

        [Test]
        public void should_be_able_to_set_permission_from_parrent()
        {
            var testFile = GetTempFilePath();
            Subject.WriteAllText(testFile, new Guid().ToString());

            Subject.InheritFolderPermissions(testFile);
        }

        [Test]
        public void should_be_set_last_file_write()
        {
            var testFile = GetTempFilePath();
            Subject.WriteAllText(testFile, new Guid().ToString());

            var lastWriteTime = DateTime.SpecifyKind(new DateTime(2012, 1, 2), DateTimeKind.Utc);

            Subject.FileSetLastWriteTime(testFile, lastWriteTime);
            Subject.FileGetLastWrite(testFile).Should().Be(lastWriteTime);
        }

        [Test]
        [Explicit]
        public void check_last_write()
        {
            Console.WriteLine(Subject.FolderGetLastWrite(GetFilledTempFolder().FullName));
            Console.WriteLine(GetFilledTempFolder().LastWriteTimeUtc);
        }

        private void VerifyCopy(string source, string destination)
        {
            var sourceFiles = Directory.GetFileSystemEntries(source, "*", SearchOption.AllDirectories).Select(v => v.Substring(source.Length + 1)).ToArray();
            var destFiles = Directory.GetFileSystemEntries(destination, "*", SearchOption.AllDirectories).Select(v => v.Substring(destination.Length + 1)).ToArray();

            CollectionAssert.AreEquivalent(sourceFiles, destFiles);
        }

        private void VerifyMove(string source, string from, string destination)
        {
            Directory.Exists(from).Should().BeFalse();

            var sourceFiles = Directory.GetFileSystemEntries(source, "*", SearchOption.AllDirectories).Select(v => v.Substring(source.Length + 1)).ToArray();
            var destFiles = Directory.GetFileSystemEntries(destination, "*", SearchOption.AllDirectories).Select(v => v.Substring(destination.Length + 1)).ToArray();

            CollectionAssert.AreEquivalent(sourceFiles, destFiles);
        }
    }
}
