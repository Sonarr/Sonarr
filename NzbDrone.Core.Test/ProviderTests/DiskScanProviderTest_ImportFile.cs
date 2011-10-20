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

namespace NzbDrone.Core.Test.ProviderTests
{
    // ReSharper disable InconsistentNaming
    public class DiskScanProviderTest_ImportFile : TestBase
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

        [TestCase("WEEDS.S03E01.DUAL.DVD.XviD.AC3.-HELLYWOOD.avi")]
        [TestCase("WEEDS.S03E01.DUAL.bluray.x264.AC3.-HELLYWOOD.mkv")]
        public void import_new_file_episode_has_better_quality_than_existing(string fileName)
        {

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeFile = Builder<EpisodeFile>.CreateNew()
                        .With(e => e.Quality = QualityTypes.SDTV).Build()
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
            VerifyFileImport(result, mocker, fakeEpisode, 12345);
            mocker.GetMock<DiskProvider>().Verify(p => p.DeleteFile(It.IsAny<string>()), Times.Once());
        }

        [TestCase("WEEDS.S03E01.DUAL.hdtv.XviD.AC3.-HELLYWOOD.avi")]
        [TestCase("WEEDS.S03E01.DUAL.DVD.XviD.AC3.-HELLYWOOD.avi")]
        [TestCase("WEEDS.S03E01.DUAL.bluray.x264.AC3.-HELLYWOOD.mkv")]
        public void import_new_multi_part_file_episode_has_equal_or_better_quality_than_existing(string fileName)
        {
            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(e => e.EpisodeFile = Builder<EpisodeFile>.CreateNew()
                                               .With(f => f.Quality = QualityTypes.SDTV)
                                               .Build())
                .Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(12345).Verifiable();

            mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.Exists(It.IsAny<String>()))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), false)).Returns(fakeEpisodes);

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            VerifyFileImport(result, mocker, fakeEpisodes[0], 12345);
            mocker.GetMock<DiskProvider>().Verify(p => p.DeleteFile(It.IsAny<string>()), Times.Once());
        }

        [TestCase("WEEDS.S03E01.DUAL.DVD.XviD.AC3.-HELLYWOOD.avi")]
        [TestCase("WEEDS.S03E01.DUAL.HDTV.XviD.AC3.-HELLYWOOD.avi")]
        public void skip_import_new_multi_part_file_episode_existing_has_better_quality(string fileName)
        {
            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(e => e.EpisodeFile = Builder<EpisodeFile>.CreateNew()
                                               .With(f => f.Quality = QualityTypes.Bluray720p)
                                               .Build())
                .Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(12345).Verifiable();

            mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.Exists(It.IsAny<String>()))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), false)).Returns(fakeEpisodes);

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            VerifySkipImport(result, mocker);
        }

        [Test]
        public void import_new_multi_part_file_episode_replace_two_files()
        {
            const string fileName = "WEEDS.S03E01E02.DUAL.bluray.x264.AC3.-HELLYWOOD.mkv";

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            var fakeEpisodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .All()
                .With(e => e.Quality = QualityTypes.SDTV)
                .Build();

            var fakeEpisode1 = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeFile = fakeEpisodeFiles[0])
                .Build();

            var fakeEpisode2 = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeFile = fakeEpisodeFiles[1])
                .Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(12345).Verifiable();

            mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.Exists(It.IsAny<String>()))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), false)).Returns(new List<Episode> { fakeEpisode1, fakeEpisode2 });

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            VerifyFileImport(result, mocker, fakeEpisode1, 12345);
            mocker.GetMock<DiskProvider>().Verify(p => p.DeleteFile(It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public void import_new_episode_no_existing_episode_file()
        {
            const string fileName = "WEEDS.S03E01E02.DUAL.bluray.x264.AC3.-HELLYWOOD.mkv";

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(e => e.EpisodeFileId = 0)
                .With(e => e.EpisodeFile = null)
                .Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(12345).Verifiable();

            mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.Exists(It.IsAny<String>()))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), false)).Returns(new List<Episode> { fakeEpisode});

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            VerifyFileImport(result, mocker, fakeEpisode, 12345);
            mocker.GetMock<DiskProvider>().Verify(p => p.DeleteFile(It.IsAny<string>()), Times.Never());
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
            mocker.GetMock<DiskProvider>().Verify(p => p.DeleteFile(It.IsAny<string>()), Times.Never());
        }
    }
}
