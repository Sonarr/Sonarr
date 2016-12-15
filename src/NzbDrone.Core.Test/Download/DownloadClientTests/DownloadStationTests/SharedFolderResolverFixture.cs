using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Clients.DownloadStation;
using NzbDrone.Core.Download.Clients.DownloadStation.Proxies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.DownloadStationTests
{
    [TestFixture]
    public class SharedFolderResolverFixture : CoreTest<SharedFolderResolver>
    {
        protected string _serialNumber = "SERIALNUMBER";
        protected OsPath _sharedFolder;
        protected OsPath _physicalPath;
        protected DownloadStationSettings _settings;

        [SetUp]
        protected void Setup()
        {
            _sharedFolder = new OsPath("/myFolder");
            _physicalPath = new OsPath("/mnt/sda1/folder");
            _settings = new DownloadStationSettings();

            Mocker.GetMock<IFileStationProxy>()
                  .Setup(f => f.GetSharedFolderMapping(It.IsAny<string>(), It.IsAny<DownloadStationSettings>()))
                  .Throws(new DownloadClientException("There is no shared folder"));

            Mocker.GetMock<IFileStationProxy>()
                  .Setup(f => f.GetSharedFolderMapping(_sharedFolder.FullPath, It.IsAny<DownloadStationSettings>()))
                  .Returns(new SharedFolderMapping(_sharedFolder.FullPath, _physicalPath.FullPath));
        }

        [Test]
        public void should_throw_when_cannot_resolve_shared_folder()
        {
            Assert.Throws(Is.InstanceOf<Exception>(), () => Subject.RemapToFullPath(new OsPath("/unknownFolder"), _settings, _serialNumber));

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_valid_sharedfolder()
        {
            var mapping = Subject.RemapToFullPath(_sharedFolder, _settings, "abc");

            mapping.Should().Be(_physicalPath);

            Mocker.GetMock<IFileStationProxy>()
                  .Verify(f => f.GetSharedFolderMapping(It.IsAny<string>(), It.IsAny<DownloadStationSettings>()), Times.Once());
        }

        [Test]
        public void should_cache_mapping()
        {
            Subject.RemapToFullPath(_sharedFolder, _settings, "abc");
            Subject.RemapToFullPath(_sharedFolder, _settings, "abc");

            Mocker.GetMock<IFileStationProxy>()
                  .Verify(f => f.GetSharedFolderMapping(It.IsAny<string>(), It.IsAny<DownloadStationSettings>()), Times.Once());
        }

        [Test]
        public void should_remap_subfolder()
        {
            var mapping = Subject.RemapToFullPath(_sharedFolder + "sub", _settings, "abc");

            mapping.Should().Be(_physicalPath + "sub");
        }
    }
}
