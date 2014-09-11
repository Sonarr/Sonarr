using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;
using FizzWare.NBuilder;

namespace NzbDrone.Core.Test.RemotePathMappingsTests
{
    [TestFixture]
    public class RemotePathMappingServiceFixture : CoreTest<RemotePathMappingService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(m => m.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IRemotePathMappingRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<RemotePathMapping>());
        }

        private void GivenMapping()
        {
            var mappings = Builder<RemotePathMapping>.CreateListOfSize(1)
                .All()
                .With(v => v.Host = "my-server.localdomain")
                .With(v => v.RemotePath = "/mnt/storage/")
                .With(v => v.LocalPath = @"D:\mountedstorage\".AsOsAgnostic())
                .BuildListOfNew();

            Mocker.GetMock<IRemotePathMappingRepository>()
                  .Setup(s => s.All())
                  .Returns(mappings);
        }

        private void WithNonExistingFolder()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(m => m.FolderExists(It.IsAny<string>()))
                .Returns(false);
        }

        [TestCase("my-first-server.localdomain", "/mnt/storage", @"D:\storage1")]
        [TestCase("my-server.localdomain", "/mnt/storage2", @"D:\storage2")]
        public void should_be_able_to_add_new_mapping(String host, String remotePath, String localPath)
        {
            GivenMapping();

            localPath = localPath.AsOsAgnostic();

            var mapping = new RemotePathMapping { Host = host, RemotePath = remotePath, LocalPath = localPath };

            Subject.Add(mapping);

            Mocker.GetMock<IRemotePathMappingRepository>().Verify(c => c.Insert(mapping), Times.Once());
        }

        [Test]
        public void should_be_able_to_remove_mapping()
        {
            Subject.Remove(1);
            Mocker.GetMock<IRemotePathMappingRepository>().Verify(c => c.Delete(1), Times.Once());
        }
        
        [TestCase("my-server.localdomain", "/mnt/storage", @"D:\mountedstorage")]
        [TestCase("my-server.localdomain", "/mnt/storage", @"D:\mountedstorage2")]
        public void adding_duplicated_mapping_should_throw(String host, String remotePath, String localPath)
        {
            localPath = localPath.AsOsAgnostic();

            GivenMapping();

            var mapping = new RemotePathMapping { Host = host, RemotePath = remotePath, LocalPath = localPath };

            Assert.Throws<InvalidOperationException>(() => Subject.Add(mapping));
        }

        [TestCase("my-server.localdomain", "/mnt/storage/downloads/tv", @"D:\mountedstorage\downloads\tv")]
        [TestCase("my-2server.localdomain", "/mnt/storage/downloads/tv", "/mnt/storage/downloads/tv")]
        [TestCase("my-server.localdomain", "/mnt/storageabc/downloads/tv", "/mnt/storageabc/downloads/tv")]
        public void should_remap_remote_to_local(String host, String remotePath, String expectedLocalPath)
        {
            expectedLocalPath = expectedLocalPath.AsOsAgnostic();

            GivenMapping();

            var result = Subject.RemapRemoteToLocal(host, remotePath);

            result.Should().Be(expectedLocalPath);
        }

        [TestCase("my-server.localdomain", "/mnt/storage/downloads/tv", @"D:\mountedstorage\downloads\tv")]
        [TestCase("my-server.localdomain", "/mnt/storage", @"D:\mountedstorage")]
        [TestCase("my-2server.localdomain", "/mnt/storage/downloads/tv", "/mnt/storage/downloads/tv")]
        [TestCase("my-server.localdomain", "/mnt/storageabc/downloads/tv", "/mnt/storageabc/downloads/tv")]
        public void should_remap_local_to_remote(String host, String expectedRemotePath, String localPath)
        {
            localPath = localPath.AsOsAgnostic();

            GivenMapping();

            var result = Subject.RemapLocalToRemote(host, localPath);

            result.Should().Be(expectedRemotePath);
        }
    }
}