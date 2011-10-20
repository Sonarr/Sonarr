// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests
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
        public void IsMonitored_should_return_true()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), true))
                .Returns(new List<Episode> { episode });

            parseResultSingle.Series.Should().BeNull();

            var result = mocker.Resolve<InventoryProvider>().IsMonitored(parseResultSingle);

            //Assert
            result.Should().BeTrue();
            parseResultSingle.Series.Should().Be(series);
            mocker.VerifyAllMocks();
        }


        [Test]
        public void IsMonitored_ignored_single_episode_should_return_false()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), true))
                .Returns(new List<Episode> { episode });

            episode.Ignored = true;

            parseResultSingle.Series.Should().BeNull();

            var result = mocker.Resolve<InventoryProvider>().IsMonitored(parseResultSingle);

            //Assert
            result.Should().BeFalse();
            parseResultSingle.Series.Should().Be(series);
            mocker.VerifyAllMocks();
        }

        [Test]
        public void IsMonitored_multi_some_episodes_ignored_should_return_true()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), true))
                .Returns(new List<Episode> { episode, episode2 });

            episode.Ignored = true;
            episode2.Ignored = false;

            parseResultMulti.Series.Should().BeNull();

            var result = mocker.Resolve<InventoryProvider>().IsMonitored(parseResultMulti);

            //Assert
            result.Should().BeTrue();
            parseResultMulti.Series.Should().Be(series);
            mocker.VerifyAllMocks();
        }

        [Test]
        public void IsMonitored_multi_all_episodes_ignored_should_return_false()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), true))
                .Returns(new List<Episode> { episode, episode2 });

            episode.Ignored = true;
            episode2.Ignored = true;

            parseResultSingle.Series.Should().BeNull();

            var result = mocker.Resolve<InventoryProvider>().IsMonitored(parseResultMulti);

            //Assert
            result.Should().BeFalse();
            parseResultMulti.Series.Should().Be(series);
            mocker.VerifyAllMocks();
        }


        [Test]
        public void IsMonitored_multi_no_episodes_ignored_should_return_true()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), true))
                .Returns(new List<Episode> { episode, episode2 });

            episode.Ignored = false;
            episode2.Ignored = false;

            parseResultSingle.Series.Should().BeNull();

            var result = mocker.Resolve<InventoryProvider>().IsMonitored(parseResultMulti);

            //Assert
            result.Should().BeTrue();
            parseResultMulti.Series.Should().Be(series);
            mocker.VerifyAllMocks();
        }


    }
}