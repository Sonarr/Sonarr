using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class DiskProviderFixture : TestBase
    {
        DirectoryInfo BinFolder;
        DirectoryInfo BinFolderCopy;
        DirectoryInfo BinFolderMove;

        [SetUp]
        public void Setup()
        {
            var binRoot = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent;
            BinFolder = new DirectoryInfo(Path.Combine(binRoot.FullName, "bin"));
            BinFolderCopy = new DirectoryInfo(Path.Combine(binRoot.FullName, "bin_copy"));
            BinFolderMove = new DirectoryInfo(Path.Combine(binRoot.FullName, "bin_move"));

            if (BinFolderCopy.Exists)
            {
                BinFolderCopy.Delete(true);
            }

            if (BinFolderMove.Exists)
            {
                BinFolderMove.Delete(true);
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
            diskProvider.CopyDirectory(BinFolder.FullName, BinFolderCopy.FullName);

            var targetPath = Path.Combine(BinFolderCopy.FullName, "file.move");

            diskProvider.MoveFile(BinFolderCopy.GetFiles("*.dll", SearchOption.AllDirectories).First().FullName, targetPath);
            diskProvider.MoveFile(BinFolderCopy.GetFiles("*.pdb", SearchOption.AllDirectories).First().FullName, targetPath);

            File.Exists(targetPath).Should().BeTrue();
        }

        [Test]
        public void moveFile_should_not_move_overwrite_itself()
        {
            var diskProvider = new DiskProvider();
            diskProvider.CopyDirectory(BinFolder.FullName, BinFolderCopy.FullName);

            var targetPath = BinFolderCopy.GetFiles("*.dll", SearchOption.AllDirectories).First().FullName;

            diskProvider.MoveFile(targetPath, targetPath);

            File.Exists(targetPath).Should().BeTrue();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void CopyFolder_should_copy_folder()
        {
            
            var diskProvider = new DiskProvider();
            diskProvider.CopyDirectory(BinFolder.FullName, BinFolderCopy.FullName);

            
            VerifyCopy();
        }


        [Test]
        public void CopyFolder_should_overright_existing_folder()
        {
            
            var diskProvider = new DiskProvider();

            diskProvider.CopyDirectory(BinFolder.FullName, BinFolderCopy.FullName);

            //Delete Random File
            BinFolderCopy.Refresh();
            BinFolderCopy.GetFiles("*.*", SearchOption.AllDirectories).First().Delete();

            diskProvider.CopyDirectory(BinFolder.FullName, BinFolderCopy.FullName);

            
            VerifyCopy();
        }

        [Test]
        public void MoveFolder_should_overright_existing_folder()
        {
            var diskProvider = new DiskProvider();

            diskProvider.CopyDirectory(BinFolder.FullName, BinFolderCopy.FullName);
            diskProvider.CopyDirectory(BinFolder.FullName, BinFolderMove.FullName);
            VerifyCopy();

            
            diskProvider.MoveDirectory(BinFolderCopy.FullName, BinFolderMove.FullName);

            
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
            BinFolder.Refresh();
            BinFolderCopy.Refresh();

            BinFolderCopy.GetFiles("*.*", SearchOption.AllDirectories)
               .Should().HaveSameCount(BinFolder.GetFiles("*.*", SearchOption.AllDirectories));

            BinFolderCopy.GetDirectories().Should().HaveSameCount(BinFolder.GetDirectories());
        }

        private void VerifyMove()
        {
            BinFolder.Refresh();
            BinFolderCopy.Refresh();
            BinFolderMove.Refresh();

            BinFolderCopy.Exists.Should().BeFalse();

            BinFolderMove.GetFiles("*.*", SearchOption.AllDirectories)
               .Should().HaveSameCount(BinFolder.GetFiles("*.*", SearchOption.AllDirectories));

            BinFolderMove.GetDirectories().Should().HaveSameCount(BinFolder.GetDirectories());
        }
    }
}
