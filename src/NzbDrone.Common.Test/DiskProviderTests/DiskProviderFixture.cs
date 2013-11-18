using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.DiskProviderTests
{
    [TestFixture]
    public class DiskProviderFixture : TestBase<DiskProvider>
    {
        DirectoryInfo _binFolder;
        DirectoryInfo _binFolderCopy;
        DirectoryInfo _binFolderMove;

        [SetUp]
        public void Setup()
        {
            _binFolder = new DirectoryInfo(Directory.GetCurrentDirectory());
            _binFolderCopy = new DirectoryInfo(Path.Combine(_binFolder.Parent.FullName, "bin_copy"));
            _binFolderMove = new DirectoryInfo(Path.Combine(_binFolder.Parent.FullName, "bin_move"));

            if (_binFolderCopy.Exists)
            {
                foreach (var file in _binFolderCopy.GetFiles("*", SearchOption.AllDirectories))
                {
                    file.Attributes = FileAttributes.Normal;
                }
                _binFolderCopy.Delete(true);
            }

            if (_binFolderMove.Exists)
            {
                _binFolderMove.Delete(true);
            }
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
        public void moveFile_should_overwrite_existing_file()
        {

            Subject.CopyFolder(_binFolder.FullName, _binFolderCopy.FullName);

            var targetPath = Path.Combine(_binFolderCopy.FullName, "file.move");

            Subject.MoveFile(_binFolderCopy.GetFiles("*.dll", SearchOption.AllDirectories).First().FullName, targetPath);
            Subject.MoveFile(_binFolderCopy.GetFiles("*.pdb", SearchOption.AllDirectories).First().FullName, targetPath);

            File.Exists(targetPath).Should().BeTrue();
        }

        [Test]
        public void moveFile_should_not_move_overwrite_itself()
        {

            Subject.CopyFolder(_binFolder.FullName, _binFolderCopy.FullName);

            var targetPath = _binFolderCopy.GetFiles("*.dll", SearchOption.AllDirectories).First().FullName;

            Subject.MoveFile(targetPath, targetPath);

            File.Exists(targetPath).Should().BeTrue();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void CopyFolder_should_copy_folder()
        {
            Subject.CopyFolder(_binFolder.FullName, _binFolderCopy.FullName);
            VerifyCopy();
        }


        [Test]
        public void CopyFolder_should_overwrite_existing_folder()
        {



            Subject.CopyFolder(_binFolder.FullName, _binFolderCopy.FullName);

            //Delete Random File
            _binFolderCopy.Refresh();
            _binFolderCopy.GetFiles("*.*", SearchOption.AllDirectories).First().Delete();

            Subject.CopyFolder(_binFolder.FullName, _binFolderCopy.FullName);


            VerifyCopy();
        }

        [Test]
        public void MoveFolder_should_overwrite_existing_folder()
        {


            Subject.CopyFolder(_binFolder.FullName, _binFolderCopy.FullName);
            Subject.CopyFolder(_binFolder.FullName, _binFolderMove.FullName);
            VerifyCopy();


            Subject.MoveFolder(_binFolderCopy.FullName, _binFolderMove.FullName);


            VerifyMove();
        }


        [Test]
        public void move_read_only_file()
        {
            var source = GetTestFilePath();
            var destination = GetTestFilePath();

            Subject.WriteAllText(source, "SourceFile");
            Subject.WriteAllText(destination, "DestinationFile");

            File.SetAttributes(source, FileAttributes.ReadOnly);
            File.SetAttributes(destination, FileAttributes.ReadOnly);

            Subject.MoveFile(source, destination);
        }




        [Test]
        public void empty_folder_should_return_folder_modified_date()
        {
            var tempfolder = new DirectoryInfo(TempFolder);
            Subject.GetLastFolderWrite(TempFolder).Should().Be(tempfolder.LastWriteTimeUtc);
        }

        [Test]
        public void folder_should_return_correct_value_for_last_write()
        {
            var testDir = Path.Combine(SandboxFolder, "LastWrite");
            var testFile = Path.Combine(testDir, Path.GetRandomFileName());

            Directory.CreateDirectory(testDir);

            TestLogger.Info("Path is: {0}", testFile);

            
            Subject.WriteAllText(testFile, "Test");

            Subject.GetLastFolderWrite(SandboxFolder).Should().BeOnOrAfter(DateTime.UtcNow.AddMinutes(-1));
            Subject.GetLastFolderWrite(SandboxFolder).Should().BeBefore(DateTime.UtcNow);
        }

        [Test]
        public void should_return_false_for_unlocked_file()
        {
            var testFile = GetTestFilePath();
            Subject.WriteAllText(testFile, new Guid().ToString());

            Subject.IsFileLocked(testFile).Should().BeFalse();
        }

        [Test]
        public void should_return_false_for_unlocked_and_readonly_file()
        {
            var testFile = GetTestFilePath();
            Subject.WriteAllText(testFile, new Guid().ToString());

            File.SetAttributes(testFile, FileAttributes.ReadOnly);

            Subject.IsFileLocked(testFile).Should().BeFalse();
        }


        [Test]
        public void should_return_true_for_unlocked_file()
        {
            var testFile = GetTestFilePath();
            Subject.WriteAllText(testFile, new Guid().ToString());

            using (var file = File.OpenWrite(testFile))
            {
                Subject.IsFileLocked(testFile).Should().BeTrue();
            }
        }


        [Test]
        public void should_be_able_to_set_permission_from_parrent()
        {
            var testFile = GetTestFilePath();
            Subject.WriteAllText(testFile, new Guid().ToString());

            Subject.InheritFolderPermissions(testFile);
        }


        [Test]
        [Explicit]
        public void check_last_write()
        {
            Console.WriteLine(Subject.GetLastFolderWrite(_binFolder.FullName));
            Console.WriteLine(_binFolder.LastWriteTimeUtc);
        }

        private void VerifyCopy()
        {
            _binFolder.Refresh();
            _binFolderCopy.Refresh();

            _binFolderCopy.GetFiles("*.*", SearchOption.AllDirectories)
               .Should().HaveSameCount(_binFolder.GetFiles("*.*", SearchOption.AllDirectories));

            _binFolderCopy.GetDirectories().Should().HaveSameCount(_binFolder.GetDirectories());
        }

        private void VerifyMove()
        {
            _binFolder.Refresh();
            _binFolderCopy.Refresh();
            _binFolderMove.Refresh();

            _binFolderCopy.Exists.Should().BeFalse();

            _binFolderMove.GetFiles("*.*", SearchOption.AllDirectories)
               .Should().HaveSameCount(_binFolder.GetFiles("*.*", SearchOption.AllDirectories));

            _binFolderMove.GetDirectories().Should().HaveSameCount(_binFolder.GetDirectories());
        }
    }
}
