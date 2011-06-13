// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using AutoMoq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class InventoryProvider_QualityNeededTest : TestBase
    {
        private Episode episode;
        private Episode episode2;
        private EpisodeFile episodeFile;
        private QualityProfile hdProfile;
        private EpisodeParseResult parseResultMulti;
        private EpisodeParseResult parseResultSingle;
        private QualityProfile sdProfile;
        private Series series;

        [SetUp]
        public new void Setup()
        {
            parseResultMulti = new EpisodeParseResult
                                   {
                                       CleanTitle = "Title",
                                       Language = LanguageType.English,
                                       Quality = new Quality(QualityTypes.Bluray720p, true),
                                       EpisodeNumbers = new List<int> { 3, 4 },
                                       SeasonNumber = 12,
                                       AirDate = DateTime.Now.AddDays(-12).Date,
                                       Episodes = new List<Episode>()
                                   };

            parseResultSingle = new EpisodeParseResult
                                    {
                                        CleanTitle = "Title",
                                        Language = LanguageType.English,
                                        Quality = new Quality(QualityTypes.Bluray720p, true),
                                        EpisodeNumbers = new List<int> { 3 },
                                        SeasonNumber = 12,
                                        AirDate = DateTime.Now.AddDays(-12).Date,
                                        Episodes = new List<Episode>()
                                    };

            episodeFile = Builder<EpisodeFile>.CreateNew().With(c => c.Quality = QualityTypes.DVD).Build();

            episode = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeNumber = parseResultMulti.EpisodeNumbers[0])
                .With(c => c.SeasonNumber = parseResultMulti.SeasonNumber)
                .With(c => c.AirDate = parseResultMulti.AirDate)
                .With(c => c.Title = "EpisodeTitle1")
                .With(c => c.EpisodeFile = episodeFile)
                .Build();

            episode2 = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeNumber = parseResultMulti.EpisodeNumbers[1])
                .With(c => c.SeasonNumber = parseResultMulti.SeasonNumber)
                .With(c => c.AirDate = parseResultMulti.AirDate)
                .With(c => c.Title = "EpisodeTitle2")
                .With(c => c.EpisodeFile = episodeFile)
                .Build();

            sdProfile = new QualityProfile
                            {
                                Name = "SD",
                                Allowed = new List<QualityTypes> { QualityTypes.SDTV, QualityTypes.DVD },
                                Cutoff = QualityTypes.DVD
                            };

            hdProfile = new QualityProfile
                            {
                                Name = "HD",
                                Allowed =
                                    new List<QualityTypes>
                                        {
                                            QualityTypes.HDTV,
                                            QualityTypes.WEBDL,
                                            QualityTypes.Bluray720p,
                                            QualityTypes.Bluray1080p
                                        },
                                Cutoff = QualityTypes.Bluray720p
                            };

            series = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(d => d.CleanTitle = parseResultMulti.CleanTitle)
                .With(c => c.QualityProfile = sdProfile)
                .Build();

            parseResultMulti.Series = series;
            parseResultSingle.Series = series;

            parseResultSingle.Episodes.Add(episode);
            parseResultMulti.Episodes.Add(episode);
            parseResultMulti.Episodes.Add(episode2);


            base.Setup();
        }

        [Test]
        public void IsQualityNeeded_report_not_in_quality_profile_should_be_skipped()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultMulti.Series.QualityProfile = sdProfile;
            parseResultMulti.Quality = new Quality(QualityTypes.HDTV, true);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsQualityNeeded(parseResultMulti);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }

        [Test]
        public void IsQualityNeeded_file_already_at_cut_off_should_be_skipped()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultMulti.Series.QualityProfile = hdProfile;
            parseResultMulti.Episodes[0].EpisodeFile.Quality = QualityTypes.Bluray720p;
            parseResultMulti.Quality = new Quality(QualityTypes.HDTV, true);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsQualityNeeded(parseResultMulti);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }


        [Test]
        public void IsQualityNeeded_file_in_history_should_be_skipped()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultSingle.Series.QualityProfile = sdProfile;
            parseResultSingle.Episodes[0].EpisodeFile.Quality = QualityTypes.SDTV;
            parseResultSingle.Quality.QualityType = QualityTypes.DVD;

            mocker.GetMock<HistoryProvider>()
                .Setup(p => p.GetBestQualityInHistory(episode.EpisodeId))
                .Returns(new Quality(QualityTypes.DVD, true));

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsQualityNeeded(parseResultSingle);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }

        [Test]
        public void IsQualityNeeded_lesser_file_in_history_should_be_downloaded()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultSingle.Series.QualityProfile = sdProfile;
            parseResultSingle.Episodes[0].EpisodeFile.Quality = QualityTypes.SDTV;
            parseResultSingle.Quality.QualityType = QualityTypes.DVD;

            mocker.GetMock<HistoryProvider>()
                .Setup(p => p.GetBestQualityInHistory(episode.EpisodeId))
                .Returns(new Quality(QualityTypes.SDTV, true));

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsQualityNeeded(parseResultSingle);

            //Assert
            Assert.IsTrue(result);
            mocker.VerifyAllMocks();
        }

        [Test]
        public void IsQualityNeeded_file_not_in_history_should_be_downloaded()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            parseResultSingle.Series.QualityProfile = sdProfile;
            parseResultSingle.Episodes[0].EpisodeFile.Quality = QualityTypes.SDTV;
            parseResultSingle.Quality.QualityType = QualityTypes.DVD;

            mocker.GetMock<HistoryProvider>()
                .Setup(p => p.GetBestQualityInHistory(episode.EpisodeId))
                .Returns<Quality>(null);

            //Act
            bool result = mocker.Resolve<InventoryProvider>().IsQualityNeeded(parseResultSingle);

            //Assert
            Assert.IsTrue(result);
            mocker.VerifyAllMocks();
        }

        //Should Download
        [TestCase(QualityTypes.SDTV, true, QualityTypes.HDTV, false, true)]
        [TestCase(QualityTypes.DVD, true, QualityTypes.Bluray720p, true, true)]
        [TestCase(QualityTypes.HDTV, false, QualityTypes.HDTV, true, true)]
        [TestCase(QualityTypes.HDTV, false, QualityTypes.HDTV, false, false)]
        [TestCase(QualityTypes.Bluray720p, true, QualityTypes.Bluray1080p, false, false)]
        [TestCase(QualityTypes.HDTV, true, QualityTypes.Bluray720p, true, true)]
        [TestCase(QualityTypes.Bluray1080p, true, QualityTypes.Bluray720p, true, false)]
        [TestCase(QualityTypes.Bluray1080p, true, QualityTypes.Bluray720p, false, false)]
        [TestCase(QualityTypes.Bluray1080p, false, QualityTypes.Bluray720p, true, false)]
        [TestCase(QualityTypes.HDTV, false, QualityTypes.Bluray720p, true, true)]
        [TestCase(QualityTypes.HDTV, true, QualityTypes.HDTV, false, false)]
        public void Is_upgrade(QualityTypes fileQuality, bool isFileProper, QualityTypes reportQuality,
                               bool isReportProper, bool excpected)
        {
            //Setup

            var currentQuality = new Quality(fileQuality, isFileProper);
            var newQuality = new Quality(reportQuality, isReportProper);

            bool result = InventoryProvider.IsUpgrade(currentQuality, newQuality, QualityTypes.Bluray720p);


            Assert.AreEqual(excpected, result);
        }
    }
}