using System;
using System.Linq;
using AutoMoq;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class RootDirProviderTest : TestBase
    {
 

        [Test]
        public void GetRootDirs()
        {
            //Setup
            var sonicRepo = MockLib.GetEmptyRepository();
            sonicRepo.Add(new RootDir {Path = @"C:\TV"});
            sonicRepo.Add(new RootDir {Path = @"C:\TV2"});

            var mocker = new AutoMoqer();

            mocker.GetMock<IRepository>()
                .Setup(f => f.All<RootDir>())
                .Returns(sonicRepo.All<RootDir>);

            //Act
            var result = mocker.Resolve<RootDirProvider>().GetAll();

            //Assert
            Assert.AreEqual(result.Count, 2);
        }

        [Test]
        public void AddRootDir()
        {
            //Setup
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyRepository());

            const string path = @"C:\TV\";

            //Act
            var rootDirProvider = mocker.Resolve<RootDirProvider>();
            rootDirProvider.Add(new RootDir {Path = path});


            //Assert
            var rootDirs = rootDirProvider.GetAll();
            Assert.IsNotEmpty(rootDirs);
            Assert.Count(1, rootDirs);
            Assert.AreEqual(path, rootDirs.First().Path);
        }

        [Test]
        public void UpdateRootDir()
        {
            //Setup
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyRepository());

            const string path = @"C:\TV2";

            //Act
            var rootDirProvider = mocker.Resolve<RootDirProvider>();
            rootDirProvider.Add(new RootDir {Path = @"C:\TV"});
            rootDirProvider.Update(new RootDir {Id = 1, Path = path});

            //Assert
            var rootDirs = rootDirProvider.GetAll();
            Assert.IsNotEmpty(rootDirs);
            Assert.Count(1, rootDirs);
            Assert.AreEqual(path, rootDirs.First().Path);
        }

        [Test]
        public void RemoveRootDir()
        {
            //Setup
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyRepository());

            //Act
            var rootDirProvider = mocker.Resolve<RootDirProvider>();
            rootDirProvider.Add(new RootDir {Path = @"C:\TV"});
            rootDirProvider.Remove(1);

            //Assert
            var rootDirs = rootDirProvider.GetAll();
            Assert.Count(0, rootDirs);
        }

        [Test]
        public void GetRootDir()
        {
            //Setup
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyRepository());

            const int id = 1;
            const string path = @"C:\TV";

            //Act
            var rootDirProvider = mocker.Resolve<RootDirProvider>();
            rootDirProvider.Add(new RootDir {Id = id, Path = path});

            //Assert
            var rootDir = rootDirProvider.GetRootDir(id);
            Assert.AreEqual(1, rootDir.Id);
            Assert.AreEqual(path, rootDir.Path);
        }

        [Test]
        public void None_existing_folder_returns_empty_list()
        {
            const string path = "d:\\bad folder";

            var mocker = new AutoMoqer();
            mocker.GetMock<DiskProvider>(MockBehavior.Strict)
                .Setup(m => m.FolderExists(path)).Returns(false);

            var result = mocker.Resolve<RootDirProvider>().GetUnmappedFolders(path);

            Assert.IsNotNull(result);
            Assert.IsEmpty(result);

            mocker.VerifyAllMocks();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void empty_folder_path_throws()
        {
            var mocker = new AutoMoqer();
            mocker.Resolve<RootDirProvider>().GetUnmappedFolders("");
        }
    }
}