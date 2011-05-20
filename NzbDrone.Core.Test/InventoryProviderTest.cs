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
        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            var parseResult = new EpisodeParseResult()
            {
                CleanTitle = "Title",
                EpisodeTitle = "EpisodeTitle",
                Language = LanguageType.English,
                Proper = true,
                Quality = QualityTypes.Bluray720,
                Episodes = new List<int> { 3 },
                SeasonNumber = 12
            };

            var series = Builder<Series>.CreateNew()
                .With(c => c.Monitored = false)
                .With(d => d.CleanTitle = parseResult.CleanTitle)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResult);

            //Assert
            Assert.IsFalse(result);
        }


        [Test]
        public void no_db_series_should_be_skipped()
        {
            var parseResult = new EpisodeParseResult()
            {
                CleanTitle = "Title",
                EpisodeTitle = "EpisodeTitle",
                Language = LanguageType.English,
                Proper = true,
                Quality = QualityTypes.Bluray720,
                Episodes = new List<int> { 3 },
                SeasonNumber = 12
            };

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns<Series>(null);

            //Act
            var result = mocker.Resolve<InventoryProvider>().IsNeeded(parseResult);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void unwannted_quality_should_be_skipped()
        {
            var parseResult = new EpisodeParseResult()
            {
                CleanTitle = "Title",
                EpisodeTitle = "EpisodeTitle",
                Language = LanguageType.English,
                Proper = true,
                Quality = QualityTypes.Bluray720,
                Episodes = new List<int> { 3 },
                SeasonNumber = 12
            };

            var series = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(d => d.CleanTitle = parseResult.CleanTitle)
                .Build();

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
        }

        [Test]
        public void unwannted_file_should_be_skipped()
        {

            var parseResult = new EpisodeParseResult()
                                  {
                                      CleanTitle = "Title",
                                      EpisodeTitle = "EpisodeTitle",
                                      Language = LanguageType.English,
                                      Proper = true,
                                      Quality = QualityTypes.Bluray720,
                                      Episodes = new List<int> { 3 },
                                      SeasonNumber = 12
                                  };

            var series = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(d => d.CleanTitle = parseResult.CleanTitle)
                .Build();

            var episode = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeNumber = parseResult.Episodes[0])
                .With(c => c.SeasonNumber = parseResult.SeasonNumber)
                .Build();



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
        }
    }




}