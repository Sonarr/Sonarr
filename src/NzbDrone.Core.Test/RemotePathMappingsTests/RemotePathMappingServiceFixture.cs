using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

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

            Mocker.GetMock<IRemotePathMappingRepository>()
                  .Setup(s => s.Insert(It.IsAny<RemotePathMapping>()))
                  .Returns<RemotePathMapping>(m => m);
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
        public void should_be_able_to_add_new_mapping(string host, string remotePath, string localPath)
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
        public void adding_duplicated_mapping_should_throw(string host, string remotePath, string localPath)
        {
            localPath = localPath.AsOsAgnostic();

            GivenMapping();

            var mapping = new RemotePathMapping { Host = host, RemotePath = remotePath, LocalPath = localPath };

            Assert.Throws<InvalidOperationException>(() => Subject.Add(mapping));
        }

        [TestCase("my-server.localdomain", "/mnt/storage/downloads/tv", @"D:\mountedstorage\downloads\tv")]
        [TestCase("My-Server.localdomain", "/mnt/storage/downloads/tv", @"D:\mountedstorage\downloads\tv")]
        [TestCase("my-2server.localdomain", "/mnt/storage/downloads/tv", "/mnt/storage/downloads/tv")]
        [TestCase("my-server.localdomain", "/mnt/storageabc/downloads/tv", "/mnt/storageabc/downloads/tv")]
        public void should_remap_remote_to_local(string host, string remotePath, string expectedLocalPath)
        {
            expectedLocalPath = expectedLocalPath.AsOsAgnostic();

            GivenMapping();

            var result = Subject.RemapRemoteToLocal(host, new OsPath(remotePath));

            result.Should().Be(expectedLocalPath);
        }

        [TestCase("my-server.localdomain", "/mnt/storage/downloads/tv", @"D:\mountedstorage\downloads\tv")]
        [TestCase("My-Server.localdomain", "/mnt/storage/downloads/tv", @"D:\mountedstorage\downloads\tv")]
        [TestCase("my-server.localdomain", "/mnt/storage/", @"D:\mountedstorage")]
        [TestCase("my-2server.localdomain", "/mnt/storage/downloads/tv", "/mnt/storage/downloads/tv")]
        [TestCase("my-server.localdomain", "/mnt/storageabc/downloads/tv", "/mnt/storageabc/downloads/tv")]
        public void should_remap_local_to_remote(string host, string expectedRemotePath, string localPath)
        {
            localPath = localPath.AsOsAgnostic();

            GivenMapping();

            var result = Subject.RemapLocalToRemote(host, new OsPath(localPath));

            result.Should().Be(expectedRemotePath);
        }

        [TestCase(@"\\server\share\with/mixed/slashes", @"\\server\share\with\mixed\slashes\")]
        [TestCase(@"D:/with/forward/slashes", @"D:\with\forward\slashes\")]
        [TestCase(@"D:/with/mixed\slashes", @"D:\with\mixed\slashes\")]
        public void should_fix_wrong_slashes_on_add(string remotePath, string cleanedPath)
        {
            GivenMapping();

            var mapping = new RemotePathMapping
            {
                Host = "my-server.localdomain",
                RemotePath = remotePath,
                LocalPath = @"D:\mountedstorage\downloads\tv".AsOsAgnostic()
            };

            var result = Subject.Add(mapping);

            result.RemotePath.Should().Be(cleanedPath);
        }
    }
}
