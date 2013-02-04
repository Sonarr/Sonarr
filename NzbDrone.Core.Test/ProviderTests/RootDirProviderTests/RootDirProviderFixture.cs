// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.RootDirProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class RootDirProviderFixture : CoreTest<RootFolderService>
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


        [TestCase("D:\\TV Shows\\")]
        [TestCase("//server//folder")]
        public void should_be_able_to_add_root_dir(string path)
        {
            var root = new RootDir { Path = path };
            Subject.Add(root);

            Mocker.GetMock<IRootFolderRepository>().Verify(c => c.Add(root), Times.Once());
        }

        [Test]
        public void should_throw_if_folder_being_added_doesnt_exist()
        {
            WithNoneExistingFolder();

            Assert.Throws<DirectoryNotFoundException>(() => Subject.Add(new RootDir { Path = "C:\\TEST" }));
        }

        [Test]
        public void should_be_able_to_remove_root_dir()
        {
            Subject.Remove(1);
            Mocker.GetMock<IRootFolderRepository>().Verify(c => c.Delete(1), Times.Once());
        }

        public void None_existing_folder_returns_empty_list()
        {
            WithNoneExistingFolder();

            Mocker.GetMock<IRootFolderRepository>().Setup(c => c.All()).Returns(new List<RootDir>());

            const string path = "d:\\bad folder";

            var result = Subject.GetUnmappedFolders(path);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
            Mocker.GetMock<DiskProvider>().Verify(c => c.GetDirectories(It.IsAny<String>()), Times.Never());
        }

        [Test]
        public void GetUnmappedFolders_throw_on_empty_folders()
        {
            Assert.Throws<ArgumentException>(() => Mocker.Resolve<RootFolderService>().GetUnmappedFolders(""));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("BAD PATH")]
        public void invalid_folder_path_throws_on_add(string path)
        {
            Assert.Throws<ArgumentException>(() =>
                    Mocker.Resolve<RootFolderService>().Add(new RootDir { Id = 0, Path = path })
                );
        }

        [Test]
        public void adding_duplicated_root_folder_should_throw()
        {
            Mocker.GetMock<IRootFolderRepository>().Setup(c => c.All()).Returns(new List<RootDir> { new RootDir { Path = "C:\\TV" } });

            Subject.Add(new RootDir { Path = @"C:\TV" });
            Assert.Throws<InvalidOperationException>(() => Subject.Add(new RootDir { Path = @"C:\TV" }));
        }
    }
}