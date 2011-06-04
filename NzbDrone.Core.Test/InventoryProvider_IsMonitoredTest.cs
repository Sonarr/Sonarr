// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
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
    public class InventoryProvider_IsMonitoredTest : TestBase
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
                                       Language = LanguageType.English,
                                       Quality = new Quality(QualityTypes.Bluray720p, true),
                                       EpisodeNumbers = new List<int> { 3, 4 },
                                       SeasonNumber = 12,
                                       AirDate = DateTime.Now.AddDays(-12).Date,
                                   };

            parseResultSingle = new EpisodeParseResult()
                                    {
                                        CleanTitle = "Title",
                                        Language = LanguageType.English,
                                        Quality = new Quality(QualityTypes.Bluray720p, true),
                                        EpisodeNumbers = new List<int> { 3 },
                                        SeasonNumber = 12,
                                        AirDate = DateTime.Now.AddDays(-12).Date,
                                    };


            episode = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeNumber = parseResultMulti.EpisodeNumbers[0])
                .With(c => c.SeasonNumber = parseResultMulti.SeasonNumber)
                .With(c => c.AirDate = parseResultMulti.AirDate)
                .With(c => c.Title = "EpisodeTitle1")
                .Build();

            episode2 = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeNumber = parseResultMulti.EpisodeNumbers[1])
                .With(c => c.SeasonNumber = parseResultMulti.SeasonNumber)
                .With(c => c.AirDate = parseResultMulti.AirDate)
                .With(c => c.Title = "EpisodeTitle2")
                .Build();


            series = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(d => d.CleanTitle = parseResultMulti.CleanTitle)
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
            var result = mocker.Resolve<InventoryProvider>().IsMonitored(parseResultMulti);

            //Assert
            Assert.IsFalse(result);
            Assert.AreSame(series, parseResultMulti.Series);
            mocker.VerifyAllMocks();
        }


        [Test]
        public void not_in_db_should_be_skipped()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns<Series>(null);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsMonitored(parseResultMulti);

            //Assert
            Assert.IsFalse(result);
            mocker.VerifyAllMocks();
        }

        
        [Test]
        public void IsMonitored_dailyshow_should_do_daily_lookup()
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

            var result = mocker.Resolve<InventoryProvider>().IsMonitored(parseResultSingle);

            //Assert
            Assert.IsTrue(result);
            Assert.AreSame(series, parseResultSingle.Series);
            mocker.VerifyAllMocks();
        }

        [Test]
        public void none_db_episode_should_be_added()
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
                .Returns<Episode>(null);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.AddEpisode(It.IsAny<Episode>()));

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsMonitored(parseResultSingle);

            //Assert
            Assert.IsTrue(result);
            Assert.AreSame(series, parseResultSingle.Series);
            parseResultSingle.Episodes.Should().HaveCount(1);
            Assert.AreEqual("TBD", parseResultSingle.Episodes[0].Title);
            mocker.VerifyAllMocks();
        }
    }
}