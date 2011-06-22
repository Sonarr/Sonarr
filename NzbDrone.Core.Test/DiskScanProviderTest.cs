using System;
using System.Collections.Generic;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using PetaPoco;

namespace NzbDrone.Core.Test
{
    // ReSharper disable InconsistentNaming
    public class DiskScanProviderTest : TestBase
    {

        [Test]
        public void scan_series_should_update_last_scan_date()
        {

            var mocker = new AutoMoqer();
            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.UpdateSeries(It.Is<Series>(s => s.LastDiskSync != null))).Verifiable();

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodeBySeries(It.IsAny<long>()))
                .Returns(new List<Episode> { new Episode() });


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



    }
}
