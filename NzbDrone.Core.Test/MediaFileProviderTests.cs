using System;
using System.Linq.Expressions;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using Moq.Linq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class MediaFileProviderTests
    {
        [Test]
        [Description("Verifies that a new file imported properly")]
        public void import_new_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";
            const int seasonNumber = 3;
            const int episodeNumner = 01;
            const int size = 12345;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew().With(c => c.SeriesId = fakeSeries.SeriesId).Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<IRepository>()
                .Setup(r => r.Exists(It.IsAny<Expression<Func<EpisodeFile, Boolean>>>())).Returns(false).Verifiable();
            mocker.GetMock<IRepository>()
                .Setup(r => r.Add(It.IsAny<EpisodeFile>())).Returns(0).Verifiable();

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, seasonNumber, episodeNumner)).Returns(fakeEpisode).
                Verifiable();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(12345).Verifiable();


            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            Assert.IsNotNull(result);
            mocker.GetMock<IRepository>().VerifyAll();
            mocker.GetMock<IRepository>().Verify(r => r.Add(result), Times.Once());
            mocker.GetMock<EpisodeProvider>().VerifyAll();
            mocker.GetMock<DiskProvider>().VerifyAll();

            //Currently can't verify this since the list of episodes are loaded
            //Dynamically by SubSonic
            //Assert.AreEqual(fakeEpisode, result.Episodes[0]);

            Assert.AreEqual(fakeEpisode.SeriesId, result.SeriesId);
            Assert.AreEqual(QualityTypes.BDRip, result.Quality);
            Assert.AreEqual(Parser.NormalizePath(fileName), result.Path);
            Assert.AreEqual(size, result.Size);
            Assert.AreEqual(false, result.Proper);
            Assert.AreNotEqual(new DateTime(), result.DateAdded);
        }

        [Test]
        [Description("Verifies that a new file imported properly")]
        public void import_new_daily_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"2011.01.10 - Denis Leary - HD TV.mkv";
            DateTime airDate = new DateTime(2011, 01, 10);
            const int size = 12345;
            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew().With(c => c.SeriesId = fakeSeries.SeriesId).Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<IRepository>()
                .Setup(r => r.Exists(It.IsAny<Expression<Func<EpisodeFile, Boolean>>>())).Returns(false).Verifiable();
            mocker.GetMock<IRepository>()
                .Setup(r => r.Add(It.IsAny<EpisodeFile>())).Returns(0).Verifiable();

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, airDate)).Returns(fakeEpisode).
                Verifiable();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(size).Verifiable();


            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            Assert.IsNotNull(result);
            mocker.GetMock<IRepository>().VerifyAll();
            mocker.GetMock<IRepository>().Verify(r => r.Add(result), Times.Once());
            mocker.GetMock<EpisodeProvider>().VerifyAll();
            mocker.GetMock<DiskProvider>().VerifyAll();

            //Currently can't verify this since the list of episodes are loaded
            //Dynamically by SubSonic
            //Assert.AreEqual(fakeEpisode, result.Episodes[0]);

            Assert.AreEqual(fakeEpisode.SeriesId, result.SeriesId);
            Assert.AreEqual(QualityTypes.HDTV, result.Quality);
            Assert.AreEqual(Parser.NormalizePath(fileName), result.Path);
            Assert.AreEqual(size, result.Size);
            Assert.AreEqual(false, result.Proper);
            Assert.AreNotEqual(new DateTime(), result.DateAdded);
        }


        [Test]
        [Description("Verifies that a new file imported properly")]
        public void import_existing_season_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"WEEDS.S03E01.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";
            const int size = 12345;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(c => c.SeriesId = fakeSeries.SeriesId)
                .With(c => c.EpisodeFileId = 12).Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<IRepository>(MockBehavior.Strict)
                .Setup(r => r.Exists(It.IsAny<Expression<Func<EpisodeFile, Boolean>>>())).Returns(true).Verifiable();

            //mocker.GetMock<EpisodeProvider>()
            //   .Setup(e => e.GetEpisode(fakeSeries.SeriesId, seasonNumber, episodeNumner)).Returns(fakeEpisode)
            // .Verifiable();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(size).Verifiable();


            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            mocker.VerifyAllMocks();
            Assert.IsNull(result);
        }

        [Test]
        [Description("Verifies that a new file imported properly")]
        public void import_sample_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"2011.01.10 - Denis Leary - sample - HD TV.mkv";
            DateTime airDate = new DateTime(2011, 01, 10);
            const int size = 12345;
            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew().With(c => c.SeriesId = fakeSeries.SeriesId).Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<IRepository>()
                .Setup(r => r.Exists(It.IsAny<Expression<Func<EpisodeFile, Boolean>>>())).Returns(false).Verifiable();
            mocker.GetMock<IRepository>()
                .Setup(r => r.Add(It.IsAny<EpisodeFile>())).Returns(0).Verifiable();

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, airDate)).Returns(fakeEpisode).
                Verifiable();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(size).Verifiable();


            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        [Description("Verifies that an existing file will skip import")]
        public void import_existing_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = "WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            //Mocks

            var mocker = new AutoMoqer();
            mocker.GetMock<IRepository>(MockBehavior.Strict)
                .Setup(r => r.Exists(It.IsAny<Expression<Func<EpisodeFile, Boolean>>>())).Returns(true).Verifiable();
            mocker.GetMock<EpisodeProvider>(MockBehavior.Strict);
            mocker.GetMock<DiskProvider>()
                 .Setup(e => e.GetSize(fileName)).Returns(500000).Verifiable();


            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            Assert.IsNull(result);
            mocker.GetMock<IRepository>().Verify(r => r.Add(result), Times.Never());
            mocker.VerifyAllMocks();
        }

        [Test]
        [Description("Verifies that a file with no episode is skipped")]
        public void import_file_with_no_episode()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = "WEEDS.S03E01.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";
            const int seasonNumber = 3;
            const int episodeNumner = 01;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            //Mocks
            var mocker = new AutoMoqer();
            mocker.GetMock<IRepository>(MockBehavior.Strict)
                .Setup(r => r.Exists(It.IsAny<Expression<Func<EpisodeFile, Boolean>>>())).Returns(false).Verifiable();

            mocker.GetMock<EpisodeProvider>(MockBehavior.Strict)
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, seasonNumber, episodeNumner)).Returns<Episode>(null).
                Verifiable();

            mocker.GetMock<DiskProvider>(MockBehavior.Strict)
                .Setup(e => e.GetSize(fileName)).Returns(90000000000);


            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            mocker.VerifyAllMocks();
            Assert.IsNull(result);
            mocker.GetMock<IRepository>().Verify(r => r.Add(result), Times.Never());
        }

        [Test]
        public void scan_series_should_update_last_scan_date()
        {

            var mocker = new AutoMoqer();
            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.UpdateSeries(It.Is<Series>(s => s.LastDiskSync != null))).Verifiable();

            mocker.Resolve<MediaFileProvider>().Scan(new Series());

            mocker.VerifyAllMocks();

        }
    }
}