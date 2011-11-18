using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.SearchProviderTests
{
    public class PerformSearchFixture : CoreTest
    {
        private Mock<IndexerBase> _episodeIndexer1 = null;
        private Mock<IndexerBase> _episodeIndexer2 = null;
        private Mock<IndexerBase> _seasonIndexer1 = null;
        private Mock<IndexerBase> _seasonIndexer2 = null;
        private Mock<IndexerBase> _partialSeasonIndexer1 = null;
        private Mock<IndexerBase> _partialSeasonIndexer2 = null;
        private Mock<IndexerBase> _brokenIndexer = null;
        private Mock<IndexerBase> _nullIndexer = null;

        private List<IndexerBase> _indexers;

        private IList<EpisodeParseResult> _parseResults;
        private Series _series;
        private IList<Episode> _episodes = null;
            
        [SetUp]
        public void Setup()
        {
            _parseResults = Builder<EpisodeParseResult>.CreateListOfSize(10)
                .Build();

            _series = Builder<Series>.CreateNew()
                .Build();

            BuildIndexers();
        }

        private void BuildIndexers()
        {
            _episodeIndexer1 = new Mock<IndexerBase>();
            _episodeIndexer1.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_parseResults).Verifiable();

            _episodeIndexer2 = new Mock<IndexerBase>();
            _episodeIndexer2.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_parseResults).Verifiable();

            _seasonIndexer1 = new Mock<IndexerBase>();
            _seasonIndexer1.Setup(c => c.FetchSeason(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(_parseResults).Verifiable();

            _seasonIndexer2 = new Mock<IndexerBase>();
            _seasonIndexer2.Setup(c => c.FetchSeason(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(_parseResults).Verifiable();

            _partialSeasonIndexer1 = new Mock<IndexerBase>();
            _partialSeasonIndexer1.Setup(c => c.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_parseResults).Verifiable();

            _partialSeasonIndexer2 = new Mock<IndexerBase>();
            _partialSeasonIndexer2.Setup(c => c.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_parseResults).Verifiable();

            _brokenIndexer = new Mock<IndexerBase>();
            _brokenIndexer.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception()).Verifiable();

            List<EpisodeParseResult> nullResult = null;

            _nullIndexer = new Mock<IndexerBase>();
            _nullIndexer.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(nullResult).Verifiable();
        }

        private void WithEpisodeIndexers()
        {
            _indexers = new List<IndexerBase>{ _episodeIndexer1.Object, _episodeIndexer2.Object };

            Mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(_indexers);
        }

        private void WithPartialSeasonIndexers()
        {
            _indexers = new List<IndexerBase> { _partialSeasonIndexer1.Object, _partialSeasonIndexer2.Object };

            Mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(_indexers);
        }

        private void WithSeasonIndexers()
        {
            _indexers = new List<IndexerBase> { _seasonIndexer1.Object, _seasonIndexer2.Object };

            Mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetEnabledIndexers())
                .Returns(_indexers);
        }

        private void WithBrokenIndexer()
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
                .Setup(s => s.GetSceneName(It.IsAny<int>())).Returns("Scene Name");
        }

        private void WithSingleEpisode()
        {
            _episodes = Builder<Episode>.CreateListOfSize(1)
                .Build();
        }

        private void WithMultipleEpisodes()
        {
            _episodes = Builder<Episode>.CreateListOfSize(30)
                .Build();
        }

        private void WithNullEpisodes()
        {
            _episodes = null;
        }

        [Test]
        public void PerformSearch_should_search_all_enabled_providers()
        {
            //Setup
            WithEpisodeIndexers();
            WithSingleEpisode();

            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(new ProgressNotification("Test"), _series, 1, _episodes);

            //Assert
            result.Should().HaveCount(20);
            _episodeIndexer1.VerifyAll();
            _episodeIndexer1.VerifyAll();
        }

        [Test]
        public void PerformSearch_should_look_for_scene_name_to_search()
        {
            //Setup
            WithSceneName();
            WithEpisodeIndexers();
            WithSingleEpisode();

            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(new ProgressNotification("Test"), _series, 1, _episodes);

            //Assert
            result.Should().HaveCount(20);
            _episodeIndexer1.VerifyAll();
            _episodeIndexer2.VerifyAll();
            
            Mocker.GetMock<SceneMappingProvider>().Verify(c => c.GetSceneName(It.IsAny<int>()),
                                                      Times.Once());
        }

        [Test]
        public void PerformSearch_broken_indexer_should_not_break_job()
        {
            //Setup
            WithBrokenIndexer();
            WithSingleEpisode();

            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(new ProgressNotification("Test"), _series, 1, _episodes);

            //Assert
            result.Should().HaveCount(20);
            ExceptionVerification.ExcpectedErrors(1);
            _episodeIndexer1.VerifyAll();
            _episodeIndexer2.VerifyAll();
            _brokenIndexer.VerifyAll();

            Mocker.GetMock<SceneMappingProvider>().Verify(c => c.GetSceneName(It.IsAny<int>()),
                                                      Times.Once());
        }

        [Test]
        public void PerformSearch_for_episode_should_call_FetchEpisode()
        {
            //Setup
            WithEpisodeIndexers();
            WithSingleEpisode();

            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(new ProgressNotification("Test"), _series, 1, _episodes);

            //Assert
            result.Should().HaveCount(20);

            _episodeIndexer1.Verify(v => v.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())
                , Times.Once());

            _episodeIndexer2.Verify(v => v.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())
                , Times.Once());
        }

        [Test]
        public void PerformSearch_for_partial_season_should_call_FetchPartialSeason()
        {
            //Setup
            WithPartialSeasonIndexers();
            WithMultipleEpisodes();

            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(new ProgressNotification("Test"), _series, 1, _episodes);

            //Assert
            result.Should().HaveCount(80);

            _partialSeasonIndexer1.Verify(v => v.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())
                , Times.Exactly(4));

            _partialSeasonIndexer2.Verify(v => v.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())
                , Times.Exactly(4));
        }

        [Test]
        public void PerformSearch_for_season_should_call_FetchSeason()
        {
            //Setup
            WithSeasonIndexers();
            WithNullEpisodes();

            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(new ProgressNotification("Test"), _series, 1, _episodes);

            //Assert
            result.Should().HaveCount(20);

            _seasonIndexer1.Verify(v => v.FetchSeason(It.IsAny<string>(), It.IsAny<int>())
                , Times.Once());

            _seasonIndexer1.Verify(v => v.FetchSeason(It.IsAny<string>(), It.IsAny<int>())
                , Times.Once());
        }

        [Test]
        public void PerformSearch_should_return_empty_list_when_results_from_indexers_are_null()
        {
            //Setup
            WithNullIndexers();
            WithSingleEpisode();

            //Act
            var result = Mocker.Resolve<SearchProvider>().PerformSearch(new ProgressNotification("Test"), _series, 1, _episodes);

            //Assert
            ExceptionVerification.ExcpectedErrors(2);
            result.Should().HaveCount(0);
        }
    }
}
