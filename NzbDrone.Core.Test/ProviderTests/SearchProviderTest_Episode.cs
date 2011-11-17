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
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SearchProviderTest_Episode : CoreTest
    {
        [Test]
        public void processResults_ParseResult_should_return_after_match()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .TheFirst(1)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int>{1})
                .Build();

            var series = Builder<Series>.CreateNew().Build();

            var episode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<InventoryProvider>()
                .Setup(c => c.IsQualityNeeded(It.IsAny<EpisodeParseResult>())).Returns(true);


            mocker.GetMock<DownloadProvider>()
                .Setup(c => c.DownloadReport(It.IsAny<EpisodeParseResult>())).Returns(true);

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);
            
            //Act
            mocker.Resolve<SearchProvider>().ProcessEpisodeSearchResults(new ProgressNotification("Test"), episode, parseResults);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<InventoryProvider>().Verify(c => c.IsQualityNeeded(It.IsAny<EpisodeParseResult>()),
                                                       Times.Once());
            mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Once());
        }

        [Test]
        public void processResults_higher_quality_should_be_called_first()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(10)
                .All().With(c => c.Quality = new Quality(QualityTypes.DVD, true))
                .Random(1)
                .With(c => c.Quality = new Quality(QualityTypes.Bluray1080p, true))
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .Build();

            var series = Builder<Series>.CreateNew().Build();

            var episode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<InventoryProvider>()
                .Setup(
                    c =>
                    c.IsQualityNeeded(It.Is<EpisodeParseResult>(d => d.Quality.QualityType == QualityTypes.Bluray1080p)))
                .Returns(true);

            mocker.GetMock<DownloadProvider>()
                .Setup(
                    c =>
                    c.DownloadReport(It.Is<EpisodeParseResult>(d => d.Quality.QualityType == QualityTypes.Bluray1080p)))
                .Returns(true);

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            mocker.Resolve<SearchProvider>().ProcessEpisodeSearchResults(new ProgressNotification("Test"), episode, parseResults);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<InventoryProvider>().Verify(c => c.IsQualityNeeded(It.IsAny<EpisodeParseResult>()),
                                                       Times.Once());
            mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Once());
        }

        [Test]
        public void processResults_when_same_quality_proper_should_be_called_first()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(20)
                .All()
                .With(c => c.Quality = new Quality(QualityTypes.DVD, false))
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .Random(1).With(c => c.Quality = new Quality(QualityTypes.DVD, true))
                .Build();

            parseResults.Where(c => c.Quality.Proper).Should().HaveCount(1);

            var series = Builder<Series>.CreateNew().Build();

            var episode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<InventoryProvider>()
                .Setup(c => c.IsQualityNeeded(It.Is<EpisodeParseResult>(p => p.Quality.Proper))).Returns(true);

            mocker.GetMock<DownloadProvider>()
                .Setup(c => c.DownloadReport(It.Is<EpisodeParseResult>(p => p.Quality.Proper))).Returns(true);

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            mocker.Resolve<SearchProvider>().ProcessEpisodeSearchResults(new ProgressNotification("Test"), episode, parseResults);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<InventoryProvider>().Verify(c => c.IsQualityNeeded(It.IsAny<EpisodeParseResult>()),
                                                       Times.Once());
            mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Once());
        }

        [Test]
        public void processResults_when_quality_is_not_needed_should_check_the_rest()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .Build();

            var series = Builder<Series>.CreateNew().Build();

            var episode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<InventoryProvider>()
                .Setup(c => c.IsQualityNeeded(It.IsAny<EpisodeParseResult>())).Returns(false);

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            mocker.Resolve<SearchProvider>().ProcessEpisodeSearchResults(new ProgressNotification("Test"), episode, parseResults);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<InventoryProvider>().Verify(c => c.IsQualityNeeded(It.IsAny<EpisodeParseResult>()),
                                                       Times.Exactly(4));
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void processResults_failed_IsNeeded_should_check_the_rest()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .Build();

            var series = Builder<Series>.CreateNew().Build();

            var episode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<InventoryProvider>()
                .Setup(c => c.IsQualityNeeded(It.IsAny<EpisodeParseResult>())).Throws(new Exception());

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            mocker.Resolve<SearchProvider>().ProcessEpisodeSearchResults(new ProgressNotification("Test"), episode, parseResults);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<InventoryProvider>().Verify(c => c.IsQualityNeeded(It.IsAny<EpisodeParseResult>()),
                                                       Times.Exactly(4));
            ExceptionVerification.ExcpectedErrors(4);
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void processResults_failed_download_should_not_check_the_rest()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .Build();

            var series = Builder<Series>.CreateNew().Build();

            var episode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<InventoryProvider>()
                .Setup(c => c.IsQualityNeeded(It.IsAny<EpisodeParseResult>())).Returns(true);

            mocker.GetMock<DownloadProvider>()
                .Setup(c => c.DownloadReport(It.IsAny<EpisodeParseResult>())).Throws(new Exception());

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            mocker.Resolve<SearchProvider>().ProcessEpisodeSearchResults(new ProgressNotification("Test"), episode, parseResults);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<InventoryProvider>().Verify(c => c.IsQualityNeeded(It.IsAny<EpisodeParseResult>()),
                                                       Times.Exactly(1));

            mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                   Times.Exactly(1));

            ExceptionVerification.ExcpectedErrors(1);
        }

        [Test]
        public void start_should_search_all_providers()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .Build();

            var series = Builder<Series>.CreateNew()
                    .Build();

            var episode = Builder<Episode>.CreateNew()
                .With(c => c.Series = series)
                .With(c => c.SeasonNumber = 12)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisode(episode.EpisodeId))
                .Returns(episode);

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            var indexer1 = new Mock<IndexerBase>();
            indexer1.Setup(c => c.FetchEpisode(episode.Series.Title, episode.SeasonNumber, episode.EpisodeNumber))
                .Returns(parseResults).Verifiable();

            var indexer2 = new Mock<IndexerBase>();
            indexer2.Setup(c => c.FetchEpisode(episode.Series.Title, episode.SeasonNumber, episode.EpisodeNumber))
                .Returns(parseResults).Verifiable();

            var indexers = new List<IndexerBase> { indexer1.Object, indexer2.Object };

            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(indexers);

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetSeries(It.IsAny<int>()))
                .Returns(episode.Series);

            mocker.GetMock<SceneMappingProvider>()
                .Setup(s => s.GetSceneName(It.IsAny<int>())).Returns("");

            //Act
            mocker.Resolve<SearchProvider>().EpisodeSearch(new ProgressNotification("Test"), episode.EpisodeId);


            //Assert
            mocker.VerifyAllMocks();

            ExceptionVerification.ExcpectedWarns(1);
            indexer1.VerifyAll();
            indexer2.VerifyAll();
        }

        [Test]
        public void start_should_use_scene_name_to_search()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .Build();

            var series = Builder<Series>.CreateNew().With(s => s.SeriesId = 71256)
                .Build();

            var episode = Builder<Episode>.CreateNew()
                .With(c => c.Series = series)
                .With(c => c.SeasonNumber = 12)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisode(episode.EpisodeId))
                .Returns(episode);

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            var indexer1 = new Mock<IndexerBase>();
            indexer1.Setup(c => c.FetchEpisode("The Daily Show", episode.SeasonNumber, episode.EpisodeNumber))
                .Returns(parseResults).Verifiable();

            var indexer2 = new Mock<IndexerBase>();
            indexer2.Setup(c => c.FetchEpisode("The Daily Show", episode.SeasonNumber, episode.EpisodeNumber))
                .Returns(parseResults).Verifiable();

            var indexers = new List<IndexerBase> { indexer1.Object, indexer2.Object };

            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(indexers);

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetSeries(It.IsAny<int>()))
                .Returns(episode.Series);

            mocker.GetMock<SceneMappingProvider>()
                .Setup(s => s.GetSceneName(71256)).Returns("The Daily Show");

            //Act
            mocker.Resolve<SearchProvider>().EpisodeSearch(new ProgressNotification("Test"), episode.EpisodeId);

            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedWarns(1);
            indexer1.VerifyAll();
            indexer2.VerifyAll();
        }

        [Test]
        public void start_failed_indexer_should_not_break_job()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .Build();

            var episode = Builder<Episode>.CreateNew()
                .With(c => c.Series = Builder<Series>.CreateNew().Build())
                .With(c => c.SeasonNumber = 12)
                .Build();

            var mocker = new AutoMoqer();

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisode(episode.EpisodeId))
                .Returns(episode);

            var indexer1 = new Mock<IndexerBase>();
            indexer1.Setup(c => c.FetchEpisode(episode.Series.Title, episode.SeasonNumber, episode.EpisodeNumber))
                .Returns(parseResults).Verifiable();


            var indexer2 = new Mock<IndexerBase>();
            indexer2.Setup(c => c.FetchEpisode(episode.Series.Title, episode.SeasonNumber, episode.EpisodeNumber))
                .Throws(new Exception()).Verifiable();

            var indexer3 = new Mock<IndexerBase>();
            indexer3.Setup(c => c.FetchEpisode(episode.Series.Title, episode.SeasonNumber, episode.EpisodeNumber))
                .Returns(parseResults).Verifiable();


            var indexers = new List<IndexerBase> { indexer1.Object, indexer2.Object, indexer3.Object };

            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(indexers);

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetSeries(It.IsAny<int>()))
                .Returns(episode.Series);

            mocker.GetMock<SceneMappingProvider>()
                .Setup(s => s.GetSceneName(It.IsAny<int>())).Returns("");

            //Act
            mocker.Resolve<SearchProvider>().EpisodeSearch(new ProgressNotification("Test"), episode.EpisodeId);


            //Assert
            mocker.VerifyAllMocks();

            ExceptionVerification.ExcpectedWarns(1);
            ExceptionVerification.ExcpectedErrors(1);
            indexer1.VerifyAll();
            indexer2.VerifyAll();
            indexer3.VerifyAll();
        }

        [Test]
        public void start_no_episode_found_should_return_with_error_logged()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisode(It.IsAny<long>()))
                .Returns<Episode>(null);

            //Act
            mocker.Resolve<SearchProvider>().EpisodeSearch(new ProgressNotification("Test"), 12);


            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedErrors(1);
        }

        [Test]
        public void episode_search_should_call_get_series()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .Build();

            var series = Builder<Series>.CreateNew().Build();

            var episode = Builder<Episode>.CreateNew()
                .With(c => c.Series = series)
                .With(c => c.SeasonNumber = 12)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisode(episode.EpisodeId))
                .Returns(episode);

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            var indexer1 = new Mock<IndexerBase>();
            indexer1.Setup(c => c.FetchEpisode(episode.Series.Title, episode.SeasonNumber, episode.EpisodeNumber))
                .Returns(parseResults).Verifiable();

            var indexers = new List<IndexerBase> { indexer1.Object };

            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(indexers);

            mocker.GetMock<SceneMappingProvider>()
                .Setup(s => s.GetSceneName(It.IsAny<int>())).Returns("");

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.GetSeries(It.IsAny<int>())).Returns(episode.Series);

            //Act
            mocker.Resolve<SearchProvider>().EpisodeSearch(new ProgressNotification("Test"), episode.EpisodeId);


            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedWarns(1);
            indexer1.VerifyAll();
        }

        [Test]
        public void processResults_should_return_false_when_series_is_null()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(1)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .Build();

            Series series = null;

            var episode = Builder<Episode>.CreateNew()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            var result = mocker.Resolve<SearchProvider>().ProcessEpisodeSearchResults(new ProgressNotification("Test"), episode, parseResults);

            //Assert
            mocker.VerifyAllMocks();
            result.Should().BeFalse();
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void processResults_should_return_false_when_seriesId_doesnt_match()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(1)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .Build();

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 100)
                .Build();

            var episode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            var result = mocker.Resolve<SearchProvider>().ProcessEpisodeSearchResults(new ProgressNotification("Test"), episode, parseResults);

            //Assert
            mocker.VerifyAllMocks();
            result.Should().BeFalse();
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void processResults_should_return_false_when_seasonNumber_doesnt_match()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(1)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .Build();

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 100)
                .Build();

            var episode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.SeasonNumber = 2)
                .With(e => e.EpisodeNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            var result = mocker.Resolve<SearchProvider>().ProcessEpisodeSearchResults(new ProgressNotification("Test"), episode, parseResults);

            //Assert
            mocker.VerifyAllMocks();
            result.Should().BeFalse();
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void processResults_should_return_false_when_episodeNumber_doesnt_match()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(1)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .Build();

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 100)
                .Build();

            var episode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 2)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(series);

            //Act
            var result = mocker.Resolve<SearchProvider>().ProcessEpisodeSearchResults(new ProgressNotification("Test"), episode, parseResults);

            //Assert
            mocker.VerifyAllMocks();
            result.Should().BeFalse();
            ExceptionVerification.ExcpectedWarns(1);
        }
    }
}