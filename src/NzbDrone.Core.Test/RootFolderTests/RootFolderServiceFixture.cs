using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.RootFolderTests
{
    [TestFixture]

    public class RootFolderServiceFixture : CoreTest<RootFolderService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(m => m.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IRootFolderRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<RootFolder>());
        }

        private void WithNonExistingFolder()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(m => m.FolderExists(It.IsAny<string>()))
                .Returns(false);
        }

        [TestCase("D:\\TV Shows\\")]
        [TestCase("//server//folder")]
        public void should_be_able_to_add_root_dir(string path)
        {
            var root = new RootFolder { Path = path.AsOsAgnostic() };

            Subject.Add(root);

            Mocker.GetMock<IRootFolderRepository>().Verify(c => c.Insert(root), Times.Once());
        }

        [Test]
        public void should_throw_if_folder_being_added_doesnt_exist()
        {
            WithNonExistingFolder();

            Assert.Throws<DirectoryNotFoundException>(() => Subject.Add(new RootFolder { Path = "C:\\TEST".AsOsAgnostic() }));
        }

        [Test]
        public void should_be_able_to_remove_root_dir()
        {
            Subject.Remove(1);
            Mocker.GetMock<IRootFolderRepository>().Verify(c => c.Delete(1), Times.Once());
        }

        [Test]
        public void should_return_empty_list_when_folder_doesnt_exist()
        {
            WithNonExistingFolder();

            Mocker.GetMock<IRootFolderRepository>().Setup(c => c.All()).Returns(new List<RootFolder>());

            const string path = "d:\\bad folder";

            var result = Subject.GetUnmappedFolders(path);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
            Mocker.GetMock<IDiskProvider>().Verify(c => c.GetDirectories(It.IsAny<String>()), Times.Never());
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
                    Mocker.Resolve<RootFolderService>().Add(new RootFolder { Id = 0, Path = path })
                );
        }

        [Test]
        public void adding_duplicated_root_folder_should_throw()
        {
            Mocker.GetMock<IRootFolderRepository>().Setup(c => c.All()).Returns(new List<RootFolder> { new RootFolder { Path = "C:\\TV".AsOsAgnostic() } });

            Assert.Throws<InvalidOperationException>(() => Subject.Add(new RootFolder { Path = @"C:\TV".AsOsAgnostic() }));
        }

        [Test]
        public void should_not_include_system_files_and_folders()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetDirectories(It.IsAny<String>()))
                  .Returns(new string[]
                           {
                               @"C:\30 Rock".AsOsAgnostic(),
                               @"C:\$Recycle.Bin".AsOsAgnostic(),
                               @"C:\.AppleDouble".AsOsAgnostic(), 
                               @"C:\Test\.AppleDouble".AsOsAgnostic()
                           });

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetAllSeries())
                  .Returns(new List<Series>());

            Subject.GetUnmappedFolders(@"C:\")
                   .Should().OnlyContain(u => u.Path == @"C:\30 Rock".AsOsAgnostic());
        }
    }
}