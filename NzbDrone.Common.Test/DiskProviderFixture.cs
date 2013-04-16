using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class DiskProviderFixture : TestBase
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
            Mocker.Resolve<DiskProvider>().FolderExists(TempFolder).Should().BeTrue();
        }

        [Test]
        public void directory_exist_should_be_able_to_find_existing_unc_share()
        {
            Mocker.Resolve<DiskProvider>().FolderExists(@"\\localhost\c$").Should().BeTrue();
        }

        [Test]
        public void directory_exist_should_not_be_able_to_find_none_existing_folder()
        {
            Mocker.Resolve<DiskProvider>().FolderExists(@"C:\ThisBetterNotExist\").Should().BeFalse();
        }

        [Test]
        public void moveFile_should_overwrite_existing_file()
        {
            var diskProvider = new DiskProvider();
            diskProvider.CopyDirectory(_binFolder.FullName, _binFolderCopy.FullName);

            var targetPath = Path.Combine(_binFolderCopy.FullName, "file.move");

            diskProvider.MoveFile(_binFolderCopy.GetFiles("*.dll", SearchOption.AllDirectories).First().FullName, targetPath);
            diskProvider.MoveFile(_binFolderCopy.GetFiles("*.pdb", SearchOption.AllDirectories).First().FullName, targetPath);

            File.Exists(targetPath).Should().BeTrue();
        }

        [Test]
        public void moveFile_should_not_move_overwrite_itself()
        {
            var diskProvider = new DiskProvider();
            diskProvider.CopyDirectory(_binFolder.FullName, _binFolderCopy.FullName);

            var targetPath = _binFolderCopy.GetFiles("*.dll", SearchOption.AllDirectories).First().FullName;

            diskProvider.MoveFile(targetPath, targetPath);

            File.Exists(targetPath).Should().BeTrue();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void CopyFolder_should_copy_folder()
        {

            var diskProvider = new DiskProvider();
            diskProvider.CopyDirectory(_binFolder.FullName, _binFolderCopy.FullName);


            VerifyCopy();
        }


        [Test]
        public void CopyFolder_should_overright_existing_folder()
        {

            var diskProvider = new DiskProvider();

            diskProvider.CopyDirectory(_binFolder.FullName, _binFolderCopy.FullName);

            //Delete Random File
            _binFolderCopy.Refresh();
            _binFolderCopy.GetFiles("*.*", SearchOption.AllDirectories).First().Delete();

            diskProvider.CopyDirectory(_binFolder.FullName, _binFolderCopy.FullName);


            VerifyCopy();
        }

        [Test]
        public void MoveFolder_should_overright_existing_folder()
        {
            var diskProvider = new DiskProvider();

            diskProvider.CopyDirectory(_binFolder.FullName, _binFolderCopy.FullName);
            diskProvider.CopyDirectory(_binFolder.FullName, _binFolderMove.FullName);
            VerifyCopy();


            diskProvider.MoveDirectory(_binFolderCopy.FullName, _binFolderMove.FullName);


            VerifyMove();
        }


        [TestCase(@"C:\", @"C:\")]
        [TestCase(@"C:\\", @"C:\")]
        [TestCase(@"c:\", @"C:\")]
        [TestCase(@"c:\Test", @"C:\Test\\")]
        [TestCase(@"c:\\\\\Test", @"C:\Test\\")]
        [TestCase(@"c:\Test\\\\", @"C:\Test\\")]
        [TestCase(@"c:\Test", @"C:\Test\\")]
        [TestCase(@"\\Server\pool", @"\\Server\pool")]
        [TestCase(@"\\Server\pool\", @"\\Server\pool")]
        [TestCase(@"\\Server\pool", @"\\Server\pool\")]
        [TestCase(@"\\Server\pool\", @"\\Server\pool\")]
        [TestCase(@"\\smallcheese\DRIVE_G\TV-C\Simspsons", @"\\smallcheese\DRIVE_G\TV-C\Simspsons")]
        public void paths_should_be_equeal(string first, string second)
        {
            DiskProvider.PathEquals(first, second).Should().BeTrue();
        }

        [TestCase(@"D:\Test", @"C:\Test\")]
        [TestCase(@"D:\Test\Test", @"C:\TestTest\")]
        public void paths_should_not_be_equeal(string first, string second)
        {
            DiskProvider.PathEquals(first, second).Should().BeFalse();
        }

        [Test]
        public void empty_folder_should_return_folder_modified_date()
        {
            var tempfolder = new DirectoryInfo(TempFolder);
            Mocker.Resolve<DiskProvider>().GetLastFolderWrite(TempFolder).Should().Be(tempfolder.LastWriteTimeUtc);
        }

        [Test]
        public void folder_should_return_correct_value_for_last_write()
        {
            var appPath = new EnvironmentProvider().WorkingDirectory;
            Mocker.Resolve<DiskProvider>().GetLastFolderWrite(appPath).Should().BeOnOrAfter(DateTime.UtcNow.AddMinutes(-10));
            Mocker.Resolve<DiskProvider>().GetLastFolderWrite(appPath).Should().BeBefore(DateTime.UtcNow);
        }

        [Test]
        [Explicit]
        public void check_last_write()
        {
            Console.WriteLine(Mocker.Resolve<DiskProvider>().GetLastFolderWrite(@"C:\DRIVERS"));
            Console.WriteLine(new DirectoryInfo(@"C:\DRIVERS").LastWriteTimeUtc);
        }

        [Test]
        public void IsChildOfPath_should_return_true_when_it_is_a_child()
        {
            Mocker.Resolve<DiskProvider>().IsChildOfPath(@"C:\Test\TV", @"C:\Test").Should().BeTrue();
        }

        [Test]
        public void IsChildOfPath_should_return_false_when_it_is_not_a_child()
        {
            Mocker.Resolve<DiskProvider>().IsChildOfPath(@"C:\NOT_Test\TV", @"C:\Test").Should().BeFalse();
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
