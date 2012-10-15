using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.DiskScanProviderTests
{
    // ReSharper disable InconsistentNaming
    public class MoveEpisodeFileFixture : CoreTest
    {
        [Test]
        public void should_not_move_file_if_source_and_destination_are_the_same_path()
        {
            var fakeSeries = Builder<Series>.CreateNew()
                    .With(s => s.SeriesId = 5)
                    .With(s => s.Title = "30 Rock")
                    .Build();

            var fakeEpisode = Builder<Episode>.CreateListOfSize(1)
                    .All()
                    .With(e => e.SeriesId = fakeSeries.SeriesId)
                    .With(e => e.SeasonNumber = 1)
                    .With(e => e.EpisodeNumber = 1)
                    .Build();

            const string filename = @"30 Rock - S01E01 - TBD";
            var fi = new FileInfo(Path.Combine(@"C:\Test\TV\30 Rock\Season 01\", filename + ".avi"));

            var file = Builder<EpisodeFile>.CreateNew()
                    .With(f => f.SeriesId = fakeSeries.SeriesId)
                    .With(f => f.Path = fi.FullName)
                    .Build();

            Mocker.GetMock<SeriesProvider>()
                .Setup(e => e.GetSeries(fakeSeries.SeriesId))
                .Returns(fakeSeries);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisodesByFileId(file.EpisodeFileId))
                .Returns(fakeEpisode);

            Mocker.GetMock<MediaFileProvider>()
                .Setup(e => e.GetNewFilename(fakeEpisode, fakeSeries.Title, It.IsAny<QualityTypes>(), It.IsAny<bool>(), It.IsAny<EpisodeFile>()))
                .Returns(filename);

            Mocker.GetMock<MediaFileProvider>()
                .Setup(e => e.CalculateFilePath(It.IsAny<Series>(), fakeEpisode.First().SeasonNumber, filename, ".avi"))
                .Returns(fi);

            //Act
            var result = Mocker.Resolve<DiskScanProvider>().MoveEpisodeFile(file, false);

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public void should_use_EpisodeFiles_quality()
        {
            var fakeSeries = Builder<Series>.CreateNew()
                    .With(s => s.SeriesId = 5)
                    .With(s => s.Title = "30 Rock")
                    .Build();

            var fakeEpisode = Builder<Episode>.CreateListOfSize(1)
                    .All()
                    .With(e => e.SeriesId = fakeSeries.SeriesId)
                    .With(e => e.SeasonNumber = 1)
                    .With(e => e.EpisodeNumber = 1)
                    .Build();

            const string filename = @"30 Rock - S01E01 - TBD";
            var fi = new FileInfo(Path.Combine(@"C:\Test\TV\30 Rock\Season 01\", filename + ".mkv"));
            var currentFilename = Path.Combine(@"C:\Test\TV\30 Rock\Season 01\", "30.Rock.S01E01.Test.WED-DL.mkv");
            const string message = "30 Rock - 1x01 - [WEBDL]";

            var file = Builder<EpisodeFile>.CreateNew()
                    .With(f => f.SeriesId = fakeSeries.SeriesId)
                    .With(f => f.Path = currentFilename)
                    .With(f => f.Quality = QualityTypes.WEBDL720p)
                    .With(f => f.Proper = false)
                    .Build();

            Mocker.GetMock<SeriesProvider>()
                .Setup(e => e.GetSeries(fakeSeries.SeriesId))
                .Returns(fakeSeries);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisodesByFileId(file.EpisodeFileId))
                .Returns(fakeEpisode);

            Mocker.GetMock<MediaFileProvider>()
                .Setup(e => e.GetNewFilename(fakeEpisode, fakeSeries.Title, It.IsAny<QualityTypes>(), It.IsAny<bool>(), It.IsAny<EpisodeFile>()))
                .Returns(filename);

            Mocker.GetMock<MediaFileProvider>()
                .Setup(e => e.CalculateFilePath(It.IsAny<Series>(), fakeEpisode.First().SeasonNumber, filename, ".mkv"))
                .Returns(fi);

            Mocker.GetMock<DownloadProvider>()
                    .Setup(s => s.GetDownloadTitle(It.Is<EpisodeParseResult>(e => e.Quality == new QualityModel{ Quality = QualityTypes.WEBDL720p, Proper = false })))
                    .Returns(message);

            Mocker.GetMock<ExternalNotificationProvider>()
                    .Setup(e => e.OnDownload("30 Rock - 1x01 - [WEBDL]", It.IsAny<Series>()));

            //Act
            var result = Mocker.Resolve<DiskScanProvider>().MoveEpisodeFile(file, true);

            //Assert
            result.Should().NotBeNull();
            Mocker.GetMock<ExternalNotificationProvider>()
                    .Verify(e => e.OnDownload("30 Rock - 1x01 - [WEBDL]", It.IsAny<Series>()), Times.Once());
        }
    }
}
