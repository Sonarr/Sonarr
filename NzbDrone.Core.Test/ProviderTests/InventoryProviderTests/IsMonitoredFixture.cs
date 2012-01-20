// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;

using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.InventoryProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class IsMonitoredFixture : CoreTest
    {
        private EpisodeParseResult parseResultMulti;
        private Series series;
        private Episode episode;
        private Episode episode2;
        private EpisodeParseResult parseResultSingle;
        private EpisodeParseResult parseResultDaily;

        [SetUp]
        public void Setup()
        {
            parseResultMulti = new EpisodeParseResult()
                                   {
                                       SeriesTitle = "Title",
                                       Language = LanguageType.English,
                                       Quality = new Quality(QualityTypes.Bluray720p, true),
                                       EpisodeNumbers = new List<int> { 3, 4 },
                                       SeasonNumber = 12,
                                       AirDate = DateTime.Now.AddDays(-12).Date,
                                   };

            parseResultSingle = new EpisodeParseResult()
                                    {
                                        SeriesTitle = "Title",
                                        Language = LanguageType.English,
                                        Quality = new Quality(QualityTypes.Bluray720p, true),
                                        EpisodeNumbers = new List<int> { 3 },
                                        SeasonNumber = 12,
                                        AirDate = DateTime.Now.AddDays(-12).Date,
                                    };

            parseResultDaily = new EpisodeParseResult()
                                    {
                                        SeriesTitle = "Title",
                                        Language = LanguageType.English,
                                        Quality = new Quality(QualityTypes.Bluray720p, true),
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
        }


        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            series.Monitored = false;

            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            //Act
            var result = Mocker.Resolve<InventoryProvider>().IsMonitored(parseResultMulti);

            //Assert
            Assert.IsFalse(result);
            Assert.AreSame(series, parseResultMulti.Series);
            Mocker.VerifyAllMocks();
        }


        [Test]
        public void not_in_db_should_be_skipped()
        {
            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns<Series>(null);

            //Act
            var result = Mocker.Resolve<InventoryProvider>().IsMonitored(parseResultMulti);

            //Assert
            Assert.IsFalse(result);
            Mocker.VerifyAllMocks();
        }


        [Test]
        public void IsMonitored_should_return_true()
        {
            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), true))
                .Returns(new List<Episode> { episode });

            parseResultSingle.Series.Should().BeNull();

            var result = Mocker.Resolve<InventoryProvider>().IsMonitored(parseResultSingle);

            //Assert
            result.Should().BeTrue();
            parseResultSingle.Series.Should().Be(series);
            Mocker.VerifyAllMocks();
        }


        [Test]
        public void IsMonitored_ignored_single_episode_should_return_false()
        {
            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), true))
                .Returns(new List<Episode> { episode });

            episode.Ignored = true;

            parseResultSingle.Series.Should().BeNull();

            var result = Mocker.Resolve<InventoryProvider>().IsMonitored(parseResultSingle);

            //Assert
            result.Should().BeFalse();
            parseResultSingle.Series.Should().Be(series);
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void IsMonitored_multi_some_episodes_ignored_should_return_true()
        {
            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), true))
                .Returns(new List<Episode> { episode, episode2 });

            episode.Ignored = true;
            episode2.Ignored = false;

            parseResultMulti.Series.Should().BeNull();

            var result = Mocker.Resolve<InventoryProvider>().IsMonitored(parseResultMulti);

            //Assert
            result.Should().BeTrue();
            parseResultMulti.Series.Should().Be(series);
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void IsMonitored_multi_all_episodes_ignored_should_return_false()
        {
            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), true))
                .Returns(new List<Episode> { episode, episode2 });

            episode.Ignored = true;
            episode2.Ignored = true;

            parseResultSingle.Series.Should().BeNull();

            var result = Mocker.Resolve<InventoryProvider>().IsMonitored(parseResultMulti);

            //Assert
            result.Should().BeFalse();
            parseResultMulti.Series.Should().Be(series);
            Mocker.VerifyAllMocks();
        }


        [Test]
        public void IsMonitored_multi_no_episodes_ignored_should_return_true()
        {
            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), true))
                .Returns(new List<Episode> { episode, episode2 });

            episode.Ignored = false;
            episode2.Ignored = false;

            parseResultSingle.Series.Should().BeNull();

            var result = Mocker.Resolve<InventoryProvider>().IsMonitored(parseResultMulti);

            //Assert
            result.Should().BeTrue();
            parseResultMulti.Series.Should().Be(series);
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void IsMonitored_daily_not_ignored_should_return_true()
        {
            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.FindSeries(It.IsAny<String>()))
                .Returns(series);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), true))
                .Returns(new List<Episode> { episode });

            episode.Ignored = false;

            var result = Mocker.Resolve<InventoryProvider>().IsMonitored(parseResultDaily);

            //Assert
            result.Should().BeTrue();
        }
    }
}