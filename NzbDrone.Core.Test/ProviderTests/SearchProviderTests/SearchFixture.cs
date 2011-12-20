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
    public class SearchFixture : CoreTest
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
            _episodeIndexer1.Setup(c => c.FetchSeason(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(parseResults);
            _episodeIndexer1.Setup(c => c.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(parseResults);


            _episodeIndexer2 = new Mock<IndexerBase>();
            _episodeIndexer2.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
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
        public void SeasonSearch_should_skip_daily_series()
        {
            //Setup
            _series.IsDaily = true;

            Mocker.GetMock<SeriesProvider>().Setup(s => s.GetSeries(1)).Returns(_series);

            //Act
            var result = Mocker.Resolve<SearchProvider>().SeasonSearch(MockNotification, _series.SeriesId, 1);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void PartialSeasonSearch_should_skip_daily_series()
        {
            //Setup
            _series.IsDaily = true;

            Mocker.GetMock<SeriesProvider>().Setup(s => s.GetSeries(1)).Returns(_series);

            //Act
            var result = Mocker.Resolve<SearchProvider>().PartialSeasonSearch(MockNotification, _series.SeriesId, 1);

            //Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void EpisodeSearch_should_skip_if_air_date_is_null()
        {
            //Setup
            _series.IsDaily = true;
            var episode = _episodes.First();
            episode.AirDate = null;
            episode.Series = _series;

            Mocker.GetMock<EpisodeProvider>().Setup(s => s.GetEpisode(episode.EpisodeId))
                .Returns(episode);

            //Act
            var result = Mocker.Resolve<SearchProvider>().EpisodeSearch(MockNotification, episode.EpisodeId);

            //Assert
            result.Should().BeFalse();
            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
