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
        public void import_new_file_should_succeed()
        {
            const string newFile = @"WEEDS.S03E01.DUAL.dvd.HELLYWOOD.avi";

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew().Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(newFile)).Returns(12345).Verifiable();

            mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.Exists(It.IsAny<String>()))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), false)).Returns(new List<Episode> { fakeEpisode });

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, newFile);

            //Assert
            VerifyFileImport(result, mocker, fakeEpisode, 12345);

        }

        [TestCase(QualityTypes.SDTV, false)]
        [TestCase(QualityTypes.DVD, true)]
        [TestCase(QualityTypes.HDTV, false)]
        public void import_new_file_with_better_same_quality_should_succeed(QualityTypes currentFileQuality, bool currentFileProper)
        {
            const string newFile = @"WEEDS.S03E01.DUAL.1080p.HELLYWOOD.mkv";
            const int size = 12345;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(e => e.EpisodeFile = Builder<EpisodeFile>.CreateNew()
                                               .With(g => g.Quality = (QualityTypes)currentFileQuality)
                                               .And(g => g.Proper = currentFileProper).Build()
                ).Build();

            var mocker = new AutoMoqer();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(newFile)).Returns(12345).Verifiable();



            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), false)).Returns(new List<Episode> { fakeEpisode });

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, newFile);

            //Assert
            VerifyFileImport(result, mocker, fakeEpisode, size);
        }




        [TestCase("WEEDS.S03E01.DUAL.DVD.XviD.AC3.-HELLYWOOD.avi")]
        [TestCase("WEEDS.S03E01.DUAL.SDTV.XviD.AC3.-HELLYWOOD.avi")]
        public void import_new_file_episode_has_same_or_better_quality_should_skip(string fileName)
        {

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeFile = Builder<EpisodeFile>.CreateNew()
                        .With(e => e.Quality = QualityTypes.Bluray720p).Build()
                     )
                .Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(12345).Verifiable();

            mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.Exists(It.IsAny<String>()))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), false)).Returns(new List<Episode> { fakeEpisode });

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            VerifySkipImport(result, mocker);
        }


        [Test]
        public void import_unparsable_file_should_skip()
        {
            const string fileName = @"WEEDS.avi";
            const int size = 12345;

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var mocker = new AutoMoqer();

            mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.Exists(It.IsAny<String>())).Returns(false);

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(size).Verifiable();

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            VerifySkipImport(result, mocker);
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void import_sample_file_should_skip()
        {
            const string fileName = @"2011.01.10 - Denis Leary - sample - HD TV.mkv";
            const int size = 12345;
            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.Exists(It.IsAny<String>())).Returns(false);

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(size).Verifiable();

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            VerifySkipImport(result, mocker);
        }

        [Test]
        public void import_existing_file_should_skip()
        {
            const string fileName = "WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";

            var fakeSeries = Builder<Series>.CreateNew().Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.Exists(It.IsAny<String>()))
                .Returns(true);

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            VerifySkipImport(result, mocker);
        }

        [Test]
        public void import_file_with_no_episode_in_db_should_skip()
        {
            //Constants
            const string fileName = "WEEDS.S03E01.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            //Mocks
            var mocker = new AutoMoqer();
            mocker.GetMock<MediaFileProvider>()
                  .Setup(p => p.Exists(It.IsAny<String>()))
                  .Returns(false);

            mocker.GetMock<DiskProvider>(MockBehavior.Strict)
                .Setup(e => e.GetSize(fileName)).Returns(90000000000);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), false))
                .Returns(new List<Episode>());


            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            VerifySkipImport(result, mocker);
        }

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



        private static void VerifyFileImport(EpisodeFile result, AutoMoqer mocker, Episode fakeEpisode, int size)
        {
            mocker.VerifyAllMocks();
            result.Should().NotBeNull();
            result.SeriesId.Should().Be(fakeEpisode.SeriesId);
            result.Size.Should().Be(size);
            result.DateAdded.Should().HaveDay(DateTime.Now.Day);
            mocker.GetMock<MediaFileProvider>().Verify(p => p.Add(It.IsAny<EpisodeFile>()), Times.Once());

            //Get the count of episodes linked
            var count = mocker.GetMock<EpisodeProvider>().Object.GetEpisodesByParseResult(null, false).Count;

            mocker.GetMock<EpisodeProvider>().Verify(p => p.UpdateEpisode(It.Is<Episode>(e => e.EpisodeFileId == result.EpisodeFileId)), Times.Exactly(count));
        }

        private static void VerifySkipImport(EpisodeFile result, AutoMoqer mocker)
        {
            mocker.VerifyAllMocks();
            result.Should().BeNull();
            mocker.GetMock<MediaFileProvider>().Verify(p => p.Add(It.IsAny<EpisodeFile>()), Times.Never());
            mocker.GetMock<EpisodeProvider>().Verify(p => p.UpdateEpisode(It.IsAny<Episode>()), Times.Never());
        }
    }
}
