﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
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

            Mocker.GetMock<IDiskProvider>()
                  .Setup(m => m.FolderWritable(It.IsAny<string>()))
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
        public void should_throw_when_adding_not_writable_folder()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(m => m.FolderWritable(It.IsAny<string>()))
                  .Returns(false);

            Assert.Throws<UnauthorizedAccessException>(() => Subject.Add(new RootFolder { Path = @"C:\TV".AsOsAgnostic() }));
        }

        [Test]
        public void should_throw_when_same_path_as_drone_factory()
        {
            var path = @"C:\TV".AsOsAgnostic();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.DownloadedEpisodesFolder)
                  .Returns(path);

            Assert.Throws<InvalidOperationException>(() => Subject.Add(new RootFolder { Path = path }));
}

        [TestCase("$recycle.bin")]
        [TestCase("system volume information")]
        [TestCase("recycler")]
        [TestCase("lost+found")]
        [TestCase(".appledb")]
        [TestCase(".appledesktop")]
        [TestCase(".appledouble")]
        [TestCase("@eadir")]
        [TestCase(".grab")]
        public void should_get_root_folder_with_subfolders_excluding_special_sub_folders(string subFolder)
        {
            var rootFolder = Builder<RootFolder>.CreateNew()
                                                .With(r => r.Path = @"C:\Test\TV")
                                                .Build();

            var subFolders = new[]
                        {
                            "Series1",
                            "Series2",
                            "Series3",
                            subFolder
                        };

            var folders = subFolders.Select(f => Path.Combine(@"C:\Test\TV", f)).ToArray();

            Mocker.GetMock<IRootFolderRepository>()
                  .Setup(s => s.Get(It.IsAny<int>()))
                  .Returns(rootFolder);

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetAllSeries())
                  .Returns(new List<Series>());

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetDirectories(rootFolder.Path))
                  .Returns(folders);

            var unmappedFolders = Subject.Get(rootFolder.Id).UnmappedFolders;

            unmappedFolders.Count.Should().BeGreaterThan(0);
            unmappedFolders.Should().NotContain(u => u.Name == subFolder);
        }
    }
}