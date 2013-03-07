// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;

using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.PostDownloadProviderTests
{
    [TestFixture]
    public class ProcessDropDirectoryFixture : CoreTest
    {
        Series fakeSeries;

        [SetUp]
        public void Setup()
        {
            fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Path = @"C:\Test\TV\30 Rock")
                .Build();
        }

        private void WithLotsOfFreeDiskSpace()
        {
            Mocker.GetMock<DiskProvider>().Setup(s => s.FreeDiskSpace(It.IsAny<string>())).Returns(1000000000);
        }

        [Test]
        public void ProcessDropFolder_should_only_process_folders_that_arent_known_series_folders()
        {
            WithLotsOfFreeDiskSpace();

            var subFolders = new[]
                                 {
                                    @"c:\drop\episode1",
                                    @"c:\drop\episode2",
                                    @"c:\drop\episode3",
                                    @"c:\drop\episode4"
                                 };

            Mocker.GetMock<DiskScanProvider>()
                .Setup(c => c.GetVideoFiles(It.IsAny<String>(), false))
                .Returns(new List<String>());

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetDirectories(It.IsAny<String>()))
                .Returns(subFolders);

            Mocker.GetMock<ISeriesRepository>()
                .Setup(c => c.SeriesPathExists(subFolders[1]))
                .Returns(true);

            Mocker.GetMock<ISeriesRepository>()
                .Setup(c => c.GetByTitle(It.IsAny<String>()))
                .Returns(fakeSeries);

            Mocker.GetMock<DiskScanProvider>()
                .Setup(c => c.Scan(It.IsAny<Series>(), It.IsAny<String>()))
                .Returns(new List<EpisodeFile>());

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetDirectorySize(It.IsAny<String>()))
                .Returns(10);

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(It.IsAny<String>()))
                .Returns(true);

            //Act
            Mocker.Resolve<PostDownloadProvider>().ProcessDropFolder(@"C:\drop\");

            //Assert
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.Scan(It.IsAny<Series>(), subFolders[0]), Times.Once());
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.Scan(It.IsAny<Series>(), subFolders[1]), Times.Never());
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.Scan(It.IsAny<Series>(), subFolders[2]), Times.Once());
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.Scan(It.IsAny<Series>(), subFolders[3]), Times.Once());
        }

        [Test]
        public void ProcessDropFolder_should_process_individual_video_files_in_drop_folder()
        {
            WithLotsOfFreeDiskSpace();

            var files = new List<String>
                                 {
                                    @"c:\drop\30 Rock - episode1.avi",
                                    @"c:\drop\30 Rock - episode2.mkv",
                                    @"c:\drop\30 Rock - episode3.mp4",
                                    @"c:\drop\30 Rock - episode4.wmv"
                                 };

            Mocker.GetMock<DiskScanProvider>()
                .Setup(c => c.GetVideoFiles(It.IsAny<String>(), false))
                .Returns(files);

            Mocker.GetMock<ISeriesRepository>()
                .Setup(c => c.GetByTitle(It.IsAny<String>()))
                .Returns(fakeSeries);

            Mocker.GetMock<DiskScanProvider>()
                .Setup(c => c.Scan(It.IsAny<Series>(), It.IsAny<String>()))
                .Returns(new List<EpisodeFile>());

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetDirectorySize(It.IsAny<String>()))
                .Returns(10);

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(It.IsAny<String>()))
                .Returns(true);

            //Act
            Mocker.Resolve<PostDownloadProvider>().ProcessDropFolder(@"C:\drop\");


            //Assert
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.ImportFile(It.IsAny<Series>(), It.IsAny<String>()), Times.Exactly(4));
        }
    }
}