// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class InventoryProviderTest : TestBase
    {
        private EpisodeParseResult parseResultMulti;
        private Series series;
        private Episode episode;
        private Episode episode2;
        private EpisodeParseResult parseResultSingle;

        [SetUp]
        public new void Setup()
        {
            parseResultMulti = new EpisodeParseResult()
                               {
                                   CleanTitle = "Title",
                                   EpisodeTitle = "EpisodeTitle",
                                   Language = LanguageType.English,
                                   Proper = true,
                                   Quality = QualityTypes.Bluray720p,
                                   EpisodeNumbers = new List<int> { 3, 4 },
                                   SeasonNumber = 12,
                                   AirDate = DateTime.Now.AddDays(-12).Date
                               };

            parseResultSingle = new EpisodeParseResult()
            {
                CleanTitle = "Title",
                EpisodeTitle = "EpisodeTitle",
                Language = LanguageType.English,
                Proper = true,
                Quality = QualityTypes.Bluray720p,
                EpisodeNumbers = new List<int> { 3 },
                SeasonNumber = 12,
                AirDate = DateTime.Now.AddDays(-12).Date
            };

            series = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(d => d.CleanTitle = parseResultMulti.CleanTitle)
                .Build();

            episode = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeNumber = parseResultMulti.EpisodeNumbers[0])
                .With(c => c.SeasonNumber = parseResultMulti.SeasonNumber)
                .With(c => c.AirDate = parseResultMulti.AirDate)
                .Build();

            episode2 = Builder<Episode>.CreateNew()
             .With(c => c.EpisodeNumber = parseResultMulti.EpisodeNumbers[1])
             .With(c => c.SeasonNumber = parseResultMulti.SeasonNumber)
             .With(c => c.AirDate = parseResultMulti.AirDate)
             .Build();

            base.Setup();

        }


        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            series.Monitored = false;

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResultMulti);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }


        [Test]
        public void no_db_series_should_be_skipped()
        {

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns<Series>(null);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResultMulti);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }

        [Test]
        public void unwannted_quality_should_be_skipped()
        {

            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<SeriesProvider>()
               .Setup(p => p.QualityWanted(series.SeriesId, parseResultMulti.Quality))
               .Returns(false);


            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResultMulti);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }


        [Test]
        public void ignored_season_should_be_skipped()
        {

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<SeriesProvider>()
               .Setup(p => p.QualityWanted(series.SeriesId, parseResultMulti.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResultMulti.SeasonNumber))
                .Returns(true);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResultMulti);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }





        [Test]
        public void unwannted_file_should_be_skipped()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisode(episode.SeriesId, episode.SeasonNumber, episode.EpisodeNumber))
                .Returns(episode);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisode(episode2.SeriesId, episode2.SeasonNumber, episode2.EpisodeNumber))
                .Returns(episode2);


            mocker.GetMock<SeriesProvider>()
               .Setup(p => p.QualityWanted(series.SeriesId, parseResultMulti.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResultMulti.SeasonNumber))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.IsNeeded(parseResultMulti, episode))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.IsNeeded(parseResultMulti, episode2))
                .Returns(false);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResultMulti);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }


        [Test]
        public void file_in_history_should_be_skipped()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisode(episode.SeriesId, episode.SeasonNumber, episode.EpisodeNumber))
                .Returns(episode);

            mocker.GetMock<SeriesProvider>()
               .Setup(p => p.QualityWanted(series.SeriesId, parseResultSingle.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResultSingle.SeasonNumber))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.IsNeeded(parseResultSingle, episode))
                .Returns(true);

            mocker.GetMock<HistoryProvider>()
                .Setup(p => p.Exists(episode.EpisodeId, parseResultSingle.Quality, parseResultSingle.Proper))
                .Returns(true);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResultSingle);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }

        [Test]
        public void dailyshow_should_do_daily_lookup()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisode(episode.SeriesId, episode.SeasonNumber, episode.EpisodeNumber))
                .Returns<Episode>(null);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisode(episode.SeriesId, episode.AirDate))
                .Returns(episode);


            mocker.GetMock<SeriesProvider>()
               .Setup(p => p.QualityWanted(series.SeriesId, parseResultSingle.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResultSingle.SeasonNumber))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.IsNeeded(parseResultSingle, episode))
                .Returns(false);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResultSingle);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }


        [Test]
        public void none_db_episode_should_be_added()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<SeriesProvider>()
               .Setup(p => p.QualityWanted(series.SeriesId, parseResultSingle.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResultSingle.SeasonNumber))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisode(episode.SeriesId, episode.SeasonNumber, episode.EpisodeNumber))
                .Returns<Episode>(null);

            mocker.GetMock<EpisodeProvider>()
              .Setup(p => p.GetEpisode(episode.SeriesId, episode.AirDate))
              .Returns<Episode>(null);

            mocker.GetMock<EpisodeProvider>()
               .Setup(p => p.AddEpisode(It.IsAny<Episode>()))
               .Returns(12);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.IsNeeded(parseResultSingle, It.IsAny<Episode>()))
                .Returns(false);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResultSingle);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }


        [Test]
        public void first_file_needed_should_return_true()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisode(episode.SeriesId, episode.SeasonNumber, episode.EpisodeNumber))
                .Returns(episode);

            mocker.GetMock<EpisodeProvider>()
             .Setup(p => p.GetEpisode(episode2.SeriesId, episode2.SeasonNumber, episode2.EpisodeNumber))
             .Returns(episode2);

            mocker.GetMock<SeriesProvider>()
               .Setup(p => p.QualityWanted(series.SeriesId, parseResultMulti.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResultMulti.SeasonNumber))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.IsNeeded(parseResultMulti, episode))
                .Returns(true);

            mocker.GetMock<HistoryProvider>()
                .Setup(p => p.Exists(episode.EpisodeId, parseResultMulti.Quality, parseResultMulti.Proper))
                .Returns(false);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResultMulti);

            //Assert
            Assert.IsTrue(result);
            Assert.Contains(parseResultMulti.Episodes, episode);
            Assert.Contains(parseResultMulti.Episodes, episode2);
            Assert.AreEqual(series, parseResultMulti.Series);
            mocker.VerifyAllMocks();
        }

        [Test]
        public void second_file_needed_should_return_true()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisode(episode.SeriesId, episode.SeasonNumber, episode.EpisodeNumber))
                .Returns(episode);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisode(episode2.SeriesId, episode2.SeasonNumber, episode2.EpisodeNumber))
                .Returns(episode2);


            mocker.GetMock<SeriesProvider>()
               .Setup(p => p.QualityWanted(series.SeriesId, parseResultMulti.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResultMulti.SeasonNumber))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.IsNeeded(parseResultMulti, episode))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.IsNeeded(parseResultMulti, episode2))
                .Returns(true);

            mocker.GetMock<HistoryProvider>()
                .Setup(p => p.Exists(episode2.EpisodeId, parseResultMulti.Quality, parseResultMulti.Proper))
                .Returns(false);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResultMulti);

            //Assert
            Assert.IsTrue(result);
            Assert.Contains(parseResultMulti.Episodes, episode);
            Assert.Contains(parseResultMulti.Episodes, episode2);
            Assert.AreEqual(series, parseResultMulti.Series);
            mocker.VerifyAllMocks();
        }
    }




}