using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SearchProviderTests
{
    public class PerformSearchFixture : CoreTest
    {
        private const string SCENE_NAME = "Scene Name";
        private const int SEASON_NUMBER = 5;
        private const int PARSE_RESULT_COUNT = 10;

        private Mock<IndexerBase> _episodeIndexer1;
        private Mock<IndexerBase> _episodeIndexer2;
        private Mock<IndexerBase> _brokenIndexer;
        private Mock<IndexerBase> _nullIndexer;

        private List<IndexerBase> _indexers;

        private Series _series;
        private IList<Episode> _episodes;

        [SetUp]
        public void Setup()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(PARSE_RESULT_COUNT)
                .Build();

            _series = Builder<Series>.CreateNew()
                .Build();

            _episodes = Builder<Episode>.CreateListOfSize(1)
                .Build();

            BuildIndexers(parseResults);

            _indexers = new List<IndexerBase> { _episodeIndexer1.Object, _episodeIndexer2.Object };

            Mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(_indexers);
        }

        private void BuildIndexers(IList<EpisodeParseResult> parseResults)
        {
            _episodeIndexer1 = new Mock<IndexerBase>();
            _episodeIndexer1.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(parseResults);
            _episodeIndexer1.Setup(c => c.FetchDailyEpisode(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(parseResults);
            _episodeIndexer1.Setup(c => c.FetchSeason(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(parseResults);
            _episodeIndexer1.Setup(c => c.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(parseResults);


            _episodeIndexer2 = new Mock<IndexerBase>();
            _episodeIndexer2.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(parseResults);
            _episodeIndexer2.Setup(c => c.FetchDailyEpisode(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(parseResults);
            _episodeIndexer2.Setup(c => c.FetchSeason(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(parseResults);
            _episodeIndexer2.Setup(c => c.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(parseResults);

            _brokenIndexer = new Mock<IndexerBase>();
            _brokenIndexer.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception());

            _nullIndexer = new Mock<IndexerBase>();
            _nullIndexer.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns<List<EpisodeParseResult>>(null);
        }

        private void WithTwoGoodOneBrokenIndexer()
        {
            _indexers = new List<IndexerBase> { _episodeIndexer1.Object, _brokenIndexer.Object, _episodeIndexer2.Object };

            Mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(_indexers);
        }

        private void WithNullIndexers()
        {
            _indexers = new List<IndexerBase> { _nullIndexer.Object, _nullIndexer.Object };

            Mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(_indexers);
        }

        private void WithSceneName()
        {
            Mocker.GetMock<SceneMappingProvider>()
                .Setup(s => s.GetSceneName(_series.SeriesId)).Returns(SCENE_NAME);
        }

        private void With30Episodes()
        {
            _episodes = Builder<Episode>.CreateListOfSize(30)
                .Build();
        }

        private void WithNullEpisodes()
        {
            _episodes = null;
        }

        private void VerifyFetchEpisode(Times times)
        {
            _episodeIndexer1.Verify(v => v.FetchEpisode(_series.Title, SEASON_NUMBER, It.IsAny<int>())
                , times);

            _episodeIndexer2.Verify(v => v.FetchEpisode(_series.Title, SEASON_NUMBER, It.IsAny<int>())
                , times);
        }

        private void VerifyFetchDailyEpisode(Times times)
        {
            _episodeIndexer1.Verify(v => v.FetchDailyEpisode(_series.Title, It.IsAny<DateTime>())
                , times);

            _episodeIndexer2.Verify(v => v.FetchDailyEpisode(_series.Title, It.IsAny<DateTime>())
                , times);
        }

        private void VerifyFetchEpisodeWithSceneName(Times times)
        {
            _episodeIndexer1.Verify(v => v.FetchEpisode(SCENE_NAME, SEASON_NUMBER, It.IsAny<int>())
                , times);

            _episodeIndexer2.Verify(v => v.FetchEpisode(SCENE_NAME, SEASON_NUMBER, It.IsAny<int>())
                , times);
        }

        private void VerifyFetchEpisodeBrokenIndexer(Times times)
        {
            _brokenIndexer.Verify(v => v.FetchEpisode(It.IsAny<string>(), SEASON_NUMBER, It.IsAny<int>())
                , times);
        }

        private void VerifyFetchPartialSeason(Times times)
        {
            _episodeIndexer1.Verify(v => v.FetchPartialSeason(_series.Title, SEASON_NUMBER, It.IsAny<int>())
                , times);

            _episodeIndexer2.Verify(v => v.FetchPartialSeason(_series.Title, SEASON_NUMBER, It.IsAny<int>())
                , times);
        }

        private void VerifyFetchSeason(Times times)
        {
            _episodeIndexer1.Verify(v => v.FetchSeason(_series.Title, SEASON_NUMBER), times);
            _episodeIndexer1.Verify(v => v.FetchSeason(_series.Title, SEASON_NUMBER), times);
        }

        [Test]
        public void PerformSearch_should_search_all_enabled_providers()
        {
            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(MockNotification, _series, SEASON_NUMBER, _episodes);

            //Assert
            VerifyFetchEpisode(Times.Once());
            result.Should().HaveCount(PARSE_RESULT_COUNT * 2);
        }

        [Test]
        public void PerformSearch_should_look_for_scene_name_to_search()
        {
            WithSceneName();

            //Act
            Mocker.Resolve<SearchProvider>().PerformSearch(MockNotification, _series, 1, _episodes);

            //Assert
            Mocker.GetMock<SceneMappingProvider>().Verify(c => c.GetSceneName(_series.SeriesId),
                                                      Times.Once());
        }

        [Test]
        public void broken_indexer_should_not_block_other_indexers()
        {
            //Setup
            WithTwoGoodOneBrokenIndexer();

            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(MockNotification, _series, SEASON_NUMBER, _episodes);

            //Assert
            result.Should().HaveCount(PARSE_RESULT_COUNT * 2);

            VerifyFetchEpisode(Times.Once());
            VerifyFetchEpisodeBrokenIndexer(Times.Once());

            Mocker.GetMock<SceneMappingProvider>().Verify(c => c.GetSceneName(_series.SeriesId),
                                                      Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void PerformSearch_for_episode_should_call_FetchEpisode()
        {
            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(MockNotification, _series, SEASON_NUMBER, _episodes);

            //Assert
            result.Should().HaveCount(PARSE_RESULT_COUNT * 2);

            VerifyFetchEpisode(Times.Once());
        }

        [Test]
        public void PerformSearch_for_daily_episode_should_call_FetchEpisode()
        {
            //Setup
            _series.IsDaily = true;

            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(MockNotification, _series, SEASON_NUMBER, _episodes);

            //Assert
            result.Should().HaveCount(PARSE_RESULT_COUNT * 2);

            VerifyFetchDailyEpisode(Times.Once());
        }

        [Test]
        public void PerformSearch_for_partial_season_should_call_FetchPartialSeason()
        {
            With30Episodes();

            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(MockNotification, _series, SEASON_NUMBER, _episodes);

            //Assert
            result.Should().HaveCount(80);
            VerifyFetchPartialSeason(Times.Exactly(4));
        }

        [Test]
        public void PerformSearch_for_season_should_call_FetchSeason()
        {
            WithNullEpisodes();

            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(MockNotification, _series, SEASON_NUMBER, _episodes);

            //Assert
            result.Should().HaveCount(20);
            VerifyFetchSeason(Times.Once());
        }

        [Test]
        public void PerformSearch_should_return_empty_list_when_results_from_indexers_are_null()
        {
            //Setup
            WithNullIndexers();

            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(MockNotification, _series, SEASON_NUMBER, _episodes);

            //Assert
            result.Should().HaveCount(0);
            ExceptionVerification.ExpectedErrors(2);
        }

        [Test]
        public void should_use_scene_name_to_search_for_episode_when_avilable()
        {
            WithSceneName();

            Mocker.Resolve<SearchProvider>().PerformSearch(MockNotification, _series, SEASON_NUMBER, _episodes);

            VerifyFetchEpisodeWithSceneName(Times.Once());
        }

    }
}
