using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.DiskTests
{
    public abstract class DiskProviderFixtureBase<TSubject> : TestBase<TSubject>
        where TSubject : class, IDiskProvider
    {
        [Test]
        [Retry(5)]
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

        protected abstract void SetWritePermissions(string path, bool writable);

        [Test]
        public void FolderWritable_should_return_true_for_writable_directory()
        {
            var tempFolder = GetTempFilePath();
            Directory.CreateDirectory(tempFolder);

            var result = Subject.FolderWritable(tempFolder);

            result.Should().BeTrue();
        }

        [Test]
        [Retry(5)]
        public void FolderWritable_should_return_false_for_unwritable_directory()
        {
            var tempFolder = GetTempFilePath();
            Directory.CreateDirectory(tempFolder);

            SetWritePermissions(tempFolder, false);
            try
            {
                var result = Subject.FolderWritable(tempFolder);

                result.Should().BeFalse();
            }
            finally
            {
                SetWritePermissions(tempFolder, true);
            }
        }

        [Test]
        [Retry(5)]
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

            Assert.Throws<IOException>(() => Subject.MoveFile(source, source, true));

            File.Exists(source).Should().BeTrue();
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
        [Retry(5)]
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
            Subject.WriteAllText(testFile, default(Guid).ToString());

            Subject.IsFileLocked(testFile).Should().BeFalse();
        }

        [Test]
        public void should_return_false_for_unlocked_and_readonly_file()
        {
            var testFile = GetTempFilePath();
            Subject.WriteAllText(testFile, default(Guid).ToString());

            File.SetAttributes(testFile, FileAttributes.ReadOnly);

            Subject.IsFileLocked(testFile).Should().BeFalse();
        }

        [Test]
        public void should_return_true_for_unlocked_file()
        {
            var testFile = GetTempFilePath();
            Subject.WriteAllText(testFile, default(Guid).ToString());

            using (var file = File.OpenWrite(testFile))
            {
                Subject.IsFileLocked(testFile).Should().BeTrue();
            }
        }

        [Test]
        public void should_be_able_to_set_permission_from_parrent()
        {
            var testFile = GetTempFilePath();
            Subject.WriteAllText(testFile, default(Guid).ToString());

            Subject.InheritFolderPermissions(testFile);
        }

        [Test]
        public void should_be_set_last_file_write()
        {
            var testFile = GetTempFilePath();
            Subject.WriteAllText(testFile, default(Guid).ToString());

            var lastWriteTime = DateTime.SpecifyKind(new DateTime(2012, 1, 2), DateTimeKind.Utc);

            Subject.FileSetLastWriteTime(testFile, lastWriteTime);
            Subject.FileGetLastWrite(testFile).Should().Be(lastWriteTime);
        }

        [Test]
        public void GetParentFolder_should_remove_trailing_slash_before_getting_parent_folder()
        {
            var path = @"C:\Test\TV\".AsOsAgnostic();
            var parent = @"C:\Test".AsOsAgnostic();

            Subject.GetParentFolder(path).Should().Be(parent);
        }

        [Test]
        public void RemoveEmptySubfolders_should_remove_nested_empty_folder()
        {
            var mainDir = GetTempFilePath();
            var subDir1 = Path.Combine(mainDir, "depth1");
            var subDir2 = Path.Combine(subDir1, "depth2");
            Directory.CreateDirectory(subDir2);

            Subject.RemoveEmptySubfolders(mainDir);

            Directory.Exists(mainDir).Should().Be(true);
            Directory.Exists(subDir1).Should().Be(false);
        }

        [Test]
        public void RemoveEmptySubfolders_should_not_remove_nested_nonempty_folder()
        {
            var mainDir = GetTempFilePath();
            var subDir1 = Path.Combine(mainDir, "depth1");
            var subDir2 = Path.Combine(subDir1, "depth2");
            var file = Path.Combine(subDir1, "file1.txt");
            Directory.CreateDirectory(subDir2);
            File.WriteAllText(file, "I should not be deleted");

            Subject.RemoveEmptySubfolders(mainDir);

            Directory.Exists(mainDir).Should().Be(true);
            Directory.Exists(subDir1).Should().Be(true);
            Directory.Exists(subDir2).Should().Be(false);
            File.Exists(file).Should().Be(true);
        }

        private void DoHardLinkRename(FileShare fileShare)
        {
            var sourceDir = GetTempFilePath();
            var source = Path.Combine(sourceDir, "test.txt");
            var destination = Path.Combine(sourceDir, "destination.txt");
            var rename = Path.Combine(sourceDir, "rename.txt");

            Directory.CreateDirectory(sourceDir);

            File.WriteAllText(source, "SourceFile");

            Subject.TryCreateHardLink(source, destination).Should().BeTrue();

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
        [Ignore("No longer behaving this way in a Windows 10 Feature Update")]
        public void should_not_be_able_to_rename_open_hardlinks_with_fileshare_none()
        {
            Assert.Throws<IOException>(() => DoHardLinkRename(FileShare.None));
        }

        [Test]
        [Ignore("No longer behaving this way in a Windows 10 Feature Update")]
        public void should_not_be_able_to_rename_open_hardlinks_with_fileshare_write()
        {
            Assert.Throws<IOException>(() => DoHardLinkRename(FileShare.Read));
        }
    }
}
