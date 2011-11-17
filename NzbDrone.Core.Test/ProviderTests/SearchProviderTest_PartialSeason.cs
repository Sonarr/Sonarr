using System;
using System.Collections.Generic;
using System.Linq;

using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SearchProviderTest_PartialSeason : CoreTest
    {
        [Test]
        public void SeasonPartialSearch_season_success()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .With(s => s.Title = "Title1")
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.Series = series)
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .Build();

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .All()
                .With(e => e.EpisodeNumbers = Builder<int>.CreateListOfSize(2).Build().ToList())
                .With(e => e.SeasonNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            var indexer1 = new Mock<IndexerBase>();
            indexer1.Setup(c => c.FetchPartialSeason(episodes[0].Series.Title, episodes[0].SeasonNumber, 0))
                .Returns(parseResults).Verifiable();

            var indexer2 = new Mock<IndexerBase>();
            indexer2.Setup(c => c.FetchPartialSeason(episodes[0].Series.Title, episodes[0].SeasonNumber, 0))
                .Returns(parseResults).Verifiable();

            var indexers = new List<IndexerBase> { indexer1.Object, indexer2.Object };

            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(indexers);

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetSeries(1)).Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(episodes);

            mocker.GetMock<SceneMappingProvider>()
                .Setup(s => s.GetSceneName(1)).Returns(String.Empty);

            mocker.GetMock<InventoryProvider>()
                .Setup(s => s.IsQualityNeeded(It.IsAny<EpisodeParseResult>())).Returns(true);

            mocker.GetMock<DownloadProvider>()
                .Setup(s => s.DownloadReport(It.IsAny<EpisodeParseResult>())).Returns(true);

            mocker.GetMock<SeriesProvider>()
                    .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            var result = mocker.Resolve<SearchProvider>().PartialSeasonSearch(notification, 1, 1);

            //Assert
            result.Should().HaveCount(16);
            mocker.VerifyAllMocks();
            mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()), Times.Exactly(8));
        }

        [Test]
        public void SeasonPartialSearch_season_no_results()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .With(s => s.Title = "Title1")
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.Series = series)
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .Build();

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .All()
                .With(e => e.EpisodeNumbers = Builder<int>.CreateListOfSize(2).Build().ToList())
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            var indexer1 = new Mock<IndexerBase>();
            indexer1.Setup(c => c.FetchPartialSeason(episodes[0].Series.Title, episodes[0].SeasonNumber, 0))
                .Returns(new List<EpisodeParseResult>()).Verifiable();

            var indexer2 = new Mock<IndexerBase>();
            indexer2.Setup(c => c.FetchPartialSeason(episodes[0].Series.Title, episodes[0].SeasonNumber, 0))
                .Returns(new List<EpisodeParseResult>()).Verifiable();

            var indexers = new List<IndexerBase> { indexer1.Object, indexer2.Object };

            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(indexers);

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetSeries(1)).Returns(series);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(episodes);

            mocker.GetMock<SceneMappingProvider>()
                .Setup(s => s.GetSceneName(1)).Returns(String.Empty);

            //Act
            var result = mocker.Resolve<SearchProvider>().PartialSeasonSearch(notification, 1, 1);

            //Assert
            result.Should().HaveCount(0);
            mocker.VerifyAllMocks();
            mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()), Times.Never());
        }

        [Test]
        public void ProcessPartialSeasonSearchResults_success()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .With(s => s.Title = "Title1")
                .Build();

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .All()
                .With(e => e.EpisodeNumbers = Builder<int>.CreateListOfSize(2).Build().ToList())
                .With(e => e.Series = series)
                .With(e => e.CleanTitle = "title")
                .With(e => e.SeasonNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            mocker.GetMock<InventoryProvider>()
                .Setup(s => s.IsQualityNeeded(It.IsAny<EpisodeParseResult>())).Returns(true);

            mocker.GetMock<DownloadProvider>()
                .Setup(s => s.DownloadReport(It.IsAny<EpisodeParseResult>())).Returns(true);

            mocker.GetMock<SeriesProvider>()
                    .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            var result = mocker.Resolve<SearchProvider>().ProcessPartialSeasonSearchResults(notification, parseResults, series, 1);

            //Assert
            result.Should().HaveCount(8);
            mocker.VerifyAllMocks();
            mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()), Times.Exactly(4));

        }

        [Test]
        public void ProcessPartialSeasonSearchResults_should_return_empty_list_when_quality_is_not_wanted()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .With(s => s.Title = "Title1")
                .Build();

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .TheFirst(1)
                .With(p => p.CleanTitle = "title")
                .With(p => p.SeasonNumber = 1)
                .With(p => p.FullSeason = true)
                .With(p => p.EpisodeNumbers = null)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            mocker.GetMock<InventoryProvider>()
                .Setup(s => s.IsQualityNeeded(It.IsAny<EpisodeParseResult>())).Returns(false);

            mocker.GetMock<SeriesProvider>()
                    .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            var result = mocker.Resolve<SearchProvider>().ProcessPartialSeasonSearchResults(notification, parseResults, series, 1);

            //Assert
            result.Should().HaveCount(0);
            mocker.VerifyAllMocks();
            mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()), Times.Never());
        }

        [Test]
        public void ProcessPartialSeasonSearchResults_should_return_empty_list_when_is_wrong_season()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .With(s => s.Title = "Title1")
                .Build();

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .TheFirst(1)
                .With(p => p.CleanTitle = "title")
                .With(p => p.SeasonNumber = 2)
                .With(p => p.FullSeason = true)
                .With(p => p.EpisodeNumbers = null)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            mocker.GetMock<SeriesProvider>()
                    .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            var result = mocker.Resolve<SearchProvider>().ProcessPartialSeasonSearchResults(notification, parseResults, series, 1);

            //Assert
            result.Should().HaveCount(0);
            mocker.VerifyAllMocks();
            mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()), Times.Never());
        }

        [Test]
        public void ProcessPartialSeasonSearchResults_should_return_empty_list_when_series_is_null()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .With(s => s.Title = "Title1")
                .Build();

            Series findSeries = null;

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .TheFirst(1)
                .With(p => p.CleanTitle = "title")
                .With(p => p.SeasonNumber = 1)
                .With(p => p.FullSeason = true)
                .With(p => p.EpisodeNumbers = null)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            mocker.GetMock<SeriesProvider>()
                    .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(findSeries);

            //Act
            var result = mocker.Resolve<SearchProvider>().ProcessPartialSeasonSearchResults(notification, parseResults, series, 1);

            //Assert
            result.Should().HaveCount(0);
            mocker.VerifyAllMocks();
            mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()), Times.Never());
        }

        [Test]
        public void ProcessPartialSeasonSearchResults_should_return_empty_list_when_series_mismatch()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .With(s => s.Title = "Title1")
                .Build();

            var findSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 2)
                .With(s => s.Title = "Title1")
                .Build();

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .TheFirst(1)
                .With(p => p.CleanTitle = "title")
                .With(p => p.SeasonNumber = 1)
                .With(p => p.FullSeason = true)
                .With(p => p.EpisodeNumbers = null)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            mocker.GetMock<SeriesProvider>()
                    .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(findSeries);

            //Act
            var result = mocker.Resolve<SearchProvider>().ProcessPartialSeasonSearchResults(notification, parseResults, series, 1);

            //Assert
            result.Should().HaveCount(0);
            mocker.VerifyAllMocks();
            mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()), Times.Never());
        }
    }
}