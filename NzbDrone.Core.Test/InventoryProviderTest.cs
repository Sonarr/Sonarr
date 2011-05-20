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
        private EpisodeParseResult parseResult;
        private Series series;
        private Episode episode;

        [SetUp]
        public new void Setup()
        {
            parseResult = new EpisodeParseResult()
                               {
                                   CleanTitle = "Title",
                                   EpisodeTitle = "EpisodeTitle",
                                   Language = LanguageType.English,
                                   Proper = true,
                                   Quality = QualityTypes.Bluray720,
                                   Episodes = new List<int> { 3 },
                                   SeasonNumber = 12,
                                   AirDate = DateTime.Now.AddDays(-12).Date

                               };

            series = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(d => d.CleanTitle = parseResult.CleanTitle)
                .Build();

            episode = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeNumber = parseResult.Episodes[0])
                .With(c => c.SeasonNumber = parseResult.SeasonNumber)
                .With(c => c.AirDate = parseResult.AirDate)
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
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResult);

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
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResult);

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
               .Setup(p => p.QualityWanted(series.SeriesId, parseResult.Quality))
               .Returns(false);


            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResult);

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
               .Setup(p => p.QualityWanted(series.SeriesId, parseResult.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResult.SeasonNumber))
                .Returns(true);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResult);

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


            mocker.GetMock<SeriesProvider>()
               .Setup(p => p.QualityWanted(series.SeriesId, parseResult.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResult.SeasonNumber))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.IsNeeded(parseResult, episode))
                .Returns(false);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResult);

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
               .Setup(p => p.QualityWanted(series.SeriesId, parseResult.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResult.SeasonNumber))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.IsNeeded(parseResult, episode))
                .Returns(true);

            mocker.GetMock<HistoryProvider>()
                .Setup(p => p.Exists(episode.EpisodeId, parseResult.Quality, parseResult.Proper))
                .Returns(true);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResult);

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
               .Setup(p => p.QualityWanted(series.SeriesId, parseResult.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResult.SeasonNumber))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.IsNeeded(parseResult, episode))
                .Returns(false);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResult);

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
               .Setup(p => p.QualityWanted(series.SeriesId, parseResult.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResult.SeasonNumber))
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
                .Setup(p => p.IsNeeded(parseResult, It.IsAny<Episode>()))
                .Returns(false);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResult);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }


        [Test]
        public void file_needed_should_return_true()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisode(episode.SeriesId, episode.SeasonNumber, episode.EpisodeNumber))
                .Returns(episode);


            mocker.GetMock<SeriesProvider>()
               .Setup(p => p.QualityWanted(series.SeriesId, parseResult.Quality))
               .Returns(true);

            mocker.GetMock<SeasonProvider>()
                .Setup(p => p.IsIgnored(series.SeriesId, parseResult.SeasonNumber))
                .Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.IsNeeded(parseResult, episode))
                .Returns(true);

            mocker.GetMock<HistoryProvider>()
                .Setup(p => p.Exists(episode.EpisodeId, parseResult.Quality, parseResult.Proper))
                .Returns(false);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResult);

            //Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(parseResult.Series);
            Assert.AreEqual(series, parseResult.Series);
            mocker.VerifyAllMocks();
        }
    }




}