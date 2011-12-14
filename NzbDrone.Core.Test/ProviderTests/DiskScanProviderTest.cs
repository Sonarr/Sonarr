using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    // ReSharper disable InconsistentNaming
    public class DiskScanProviderTest : CoreTest
    {
        [Test]
        public void scan_series_should_update_the_last_scan_date()
        {

            var mocker = new AutoMoqer();
            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.UpdateSeries(It.Is<Series>(s => s.LastDiskSync != null))).Verifiable();

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodeBySeries(It.IsAny<long>()))
                .Returns(new List<Episode> { new Episode() });

            mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(It.IsAny<string>()))
                .Returns(true);

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetSeriesFiles(It.IsAny<int>()))
                .Returns(new List<EpisodeFile>());

            mocker.Resolve<DiskScanProvider>().Scan(new Series());

            mocker.VerifyAllMocks();

        }

        [Test]
        public void cleanup_should_skip_existing_files()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var episodes = Builder<EpisodeFile>.CreateListOfSize(10).Build();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.FileExists(It.IsAny<String>()))
                .Returns(true);


            //Act
            mocker.Resolve<DiskScanProvider>().CleanUp(episodes);

            //Assert
            mocker.VerifyAllMocks();
        }

        [Test]
        public void cleanup_should_delete_none_existing_files()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var episodes = Builder<EpisodeFile>.CreateListOfSize(10).Build();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.FileExists(It.IsAny<String>()))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisodesByFileId(It.IsAny<int>()))
                .Returns(new List<Episode>());

            mocker.GetMock<MediaFileProvider>()
                .Setup(e => e.Delete(It.IsAny<int>()));


            //Act
            mocker.Resolve<DiskScanProvider>().CleanUp(episodes);

            //Assert
            mocker.VerifyAllMocks();

            mocker.GetMock<EpisodeProvider>()
               .Verify(e => e.GetEpisodesByFileId(It.IsAny<int>()), Times.Exactly(10));

            mocker.GetMock<MediaFileProvider>()
                .Verify(e => e.Delete(It.IsAny<int>()), Times.Exactly(10));

        }

        [Test]
        public void cleanup_should_delete_none_existing_files_remove_links_to_episodes()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var episodes = Builder<EpisodeFile>.CreateListOfSize(10).Build();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.FileExists(It.IsAny<String>()))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisodesByFileId(It.IsAny<int>()))
                .Returns(new List<Episode> { new Episode { EpisodeFileId = 10 }, new Episode { EpisodeFileId = 10 } });

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.UpdateEpisode(It.IsAny<Episode>()));

            mocker.GetMock<MediaFileProvider>()
                .Setup(e => e.Delete(It.IsAny<int>()));


            //Act
            mocker.Resolve<DiskScanProvider>().CleanUp(episodes);

            //Assert
            mocker.VerifyAllMocks();

            mocker.GetMock<EpisodeProvider>()
               .Verify(e => e.GetEpisodesByFileId(It.IsAny<int>()), Times.Exactly(10));

            mocker.GetMock<EpisodeProvider>()
                .Verify(e => e.UpdateEpisode(It.Is<Episode>(g=>g.EpisodeFileId == 0)), Times.Exactly(20));

            mocker.GetMock<MediaFileProvider>()
                .Verify(e => e.Delete(It.IsAny<int>()), Times.Exactly(10));

            mocker.GetMock<MediaFileProvider>()
                .Verify(e => e.Delete(It.IsAny<int>()), Times.Exactly(10));

        }

        [Test]
        public void scan_series_should_log_warning_if_path_doesnt_exist_on_disk()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var series = Builder<Series>.CreateNew()
                .With(s => s.Path = @"C:\Test\TV\SeriesName\")
                .Build();

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.DeleteOrphaned())
                .Returns(0);

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.RepairLinks())
                .Returns(0);    

            mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(series.Path))
                .Returns(false);

            //Act
            mocker.Resolve<DiskScanProvider>().Scan(series, series.Path);

            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void move_should_not_move_file_if_source_and_destination_are_the_same_path()
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
                .Setup(e => e.GetNewFilename(fakeEpisode, fakeSeries.Title, It.IsAny<QualityTypes>()))
                .Returns(filename);

            Mocker.GetMock<MediaFileProvider>()
                .Setup(e => e.CalculateFilePath(It.IsAny<Series>(), fakeEpisode.First().SeasonNumber, filename, ".avi"))
                .Returns(fi);

            //Act
            var result = Mocker.Resolve<DiskScanProvider>().MoveEpisodeFile(file, false);

            //Assert
            result.Should().BeFalse();
        }
    }
}
