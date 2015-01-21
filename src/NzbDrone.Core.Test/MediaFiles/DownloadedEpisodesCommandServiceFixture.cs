using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class DownloadedEpisodesCommandServiceFixture : CoreTest<DownloadedEpisodesCommandService>
    {
        private string _droneFactory = "c:\\drop\\".AsOsAgnostic();

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IConfigService>().SetupGet(c => c.DownloadedEpisodesFolder)
                  .Returns(_droneFactory);

            Mocker.GetMock<IDownloadedEpisodesImportService>()
                .Setup(v => v.ProcessRootFolder(It.IsAny<DirectoryInfo>()))
                .Returns(new List<ImportResult>());
        }

        [Test]
        public void should_process_dronefactory_if_path_is_not_specified()
        {
            Subject.Execute(new DownloadedEpisodesScanCommand());

            Mocker.GetMock<IDownloadedEpisodesImportService>().Verify(c => c.ProcessRootFolder(It.IsAny<DirectoryInfo>()), Times.Once());
        }

        [Test]
        public void should_skip_import_if_dronefactory_doesnt_exist()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists(It.IsAny<string>())).Returns(false);

            Subject.Execute(new DownloadedEpisodesScanCommand());

            Mocker.GetMock<IDownloadedEpisodesImportService>().Verify(c => c.ProcessRootFolder(It.IsAny<DirectoryInfo>()), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }       
    }
}
