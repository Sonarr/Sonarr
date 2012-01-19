// ReSharper disable RedundantUsingDirective

using System;
using System.IO;
using System.Linq;

using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class RootDirProviderTest : CoreTest
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(m => m.FolderExists(It.IsAny<string>()))
                .Returns(true);
        }

        private void WithNoneExistingFolder()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(m => m.FolderExists(It.IsAny<string>()))
                .Returns(false);
        }


        [Test]
        public void GetRootDir_should_return_all_existing_roots()
        {
            WithRealDb();

            Db.Insert(new RootDir { Path = @"C:\TV" });
            Db.Insert(new RootDir { Path = @"C:\TV2" });

            var result = Mocker.Resolve<RootDirProvider>().GetAll();
            result.Should().HaveCount(2);
        }


        [TestCase("D:\\TV Shows\\")]
        [TestCase("//server//folder")]
        public void should_be_able_to_add_root_dir(string path)
        {
            WithRealDb();

            //Act
            var rootDirProvider = Mocker.Resolve<RootDirProvider>();
            rootDirProvider.Add(new RootDir { Path = path });

            //Assert
            var rootDirs = rootDirProvider.GetAll();
            rootDirs.Should().HaveCount(1);
            rootDirs.First().Path.Should().Be(path);
        }

        [Test]
        public void should_throw_if_folder_being_added_doesnt_exist()
        {
            WithNoneExistingFolder();

            var rootDirProvider = Mocker.Resolve<RootDirProvider>();
            Assert.Throws<DirectoryNotFoundException>(() => rootDirProvider.Add(new RootDir { Path = "C:\\TEST" }));
        }


        [Test]
        public void should_be_able_to_remove_root_dir()
        {
            WithRealDb();

            //Act
            var rootDirProvider = Mocker.Resolve<RootDirProvider>();
            rootDirProvider.Add(new RootDir { Path = @"C:\TV" });
            rootDirProvider.Add(new RootDir { Path = @"C:\TV2" });
            rootDirProvider.Remove(1);

            //Assert
            var rootDirs = rootDirProvider.GetAll();
            rootDirs.Should().HaveCount(1);
        }


        [Test]
        public void None_existing_folder_returns_empty_list()
        {
            WithNoneExistingFolder();

            const string path = "d:\\bad folder";

            var result = Mocker.Resolve<RootDirProvider>().GetUnmappedFolders(path);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
            Mocker.GetMock<DiskProvider>().Verify(c => c.GetDirectories(It.IsAny<String>()), Times.Never());
        }

        [Test]
        public void GetUnmappedFolders_throw_on_empty_folders()
        {
            Assert.Throws<ArgumentException>(() => Mocker.Resolve<RootDirProvider>().GetUnmappedFolders(""));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("BAD PATH")]
        public void invalid_folder_path_throws_on_add(string path)
        {
            Assert.Throws<ArgumentException>(() =>
                    Mocker.Resolve<RootDirProvider>().Add(new RootDir { Id = 0, Path = path })
                );
        }

        [Test]
        public void adding_duplicated_root_folder_should_throw()
        {
            WithRealDb();

            //Act
            var rootDirProvider = Mocker.Resolve<RootDirProvider>();
            rootDirProvider.Add(new RootDir { Path = @"C:\TV" });
            Assert.Throws<InvalidOperationException>(() => rootDirProvider.Add(new RootDir { Path = @"C:\TV" }));
        }

    }
}