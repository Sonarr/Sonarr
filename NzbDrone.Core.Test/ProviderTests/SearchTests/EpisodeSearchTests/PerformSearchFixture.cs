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
using NzbDrone.Core.Providers.Search;
using NzbDrone.Core.Repository;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SearchTests.EpisodeSearchTests
{
    [TestFixture]
    public class PerformSearchFixture : PerformSearchTestBase
    {
        [Test]
        public void should_throw_if_episode_is_null()
        {
            Episode nullEpisode = null;
            Assert.Throws<ArgumentException>(() => 
                                                Mocker.Resolve<EpisodeSearch>()
                                                      .PerformSearch(_series, new { Episode = nullEpisode }, notification));
        }

        [Test]
        public void should_fetch_results_from_indexers()
        {
            WithValidIndexers();

            Mocker.Resolve<EpisodeSearch>()
                  .PerformSearch(_series, new {Episode = _episode}, notification)
                  .Should()
                  .HaveCount(20);
        }

        [Test]
        public void should_log_error_when_fetching_from_indexer_fails()
        {
            WithInvalidIndexers();

            Mocker.Resolve<EpisodeSearch>()
                  .PerformSearch(_series, new { Episode = _episode }, notification)
                  .Should()
                  .HaveCount(0);

            ExceptionVerification.ExpectedErrors(2);
        }

        [Test]
        public void should_use_scene_numbering_when_available()
        {
            _series.UseSceneNumbering = true;
            _episode.SceneEpisodeNumber = 5;
            _episode.SceneSeasonNumber = 10;

            WithValidIndexers();

            Mocker.Resolve<EpisodeSearch>()
                  .PerformSearch(_series, new { Episode = _episode }, notification)
                  .Should()
                  .HaveCount(20);

            _indexer1.Verify(v => v.FetchEpisode(_series.Title, 10, 5), Times.Once());
            _indexer2.Verify(v => v.FetchEpisode(_series.Title, 10, 5), Times.Once());
        }

        [Test]
        public void should_use_standard_numbering_when_scene_series_set_but_info_is_not_available()
        {
            _series.UseSceneNumbering = true;
            _episode.SceneEpisodeNumber = 0;
            _episode.SceneSeasonNumber = 0;

            WithValidIndexers();

            Mocker.Resolve<EpisodeSearch>()
                  .PerformSearch(_series, new { Episode = _episode }, notification)
                  .Should()
                  .HaveCount(20);

            _indexer1.Verify(v => v.FetchEpisode(_series.Title, _episode.SeasonNumber, _episode.EpisodeNumber), Times.Once());
            _indexer2.Verify(v => v.FetchEpisode(_series.Title, _episode.SeasonNumber, _episode.EpisodeNumber), Times.Once());
        }

        [Test]
        public void should_use_standard_numbering_when_not_scene_series()
        {
            _series.UseSceneNumbering = false;

            WithValidIndexers();

            Mocker.Resolve<EpisodeSearch>()
                  .PerformSearch(_series, new { Episode = _episode }, notification)
                  .Should()
                  .HaveCount(20);

            _indexer1.Verify(v => v.FetchEpisode(_series.Title, _episode.SeasonNumber, _episode.EpisodeNumber), Times.Once());
            _indexer2.Verify(v => v.FetchEpisode(_series.Title, _episode.SeasonNumber, _episode.EpisodeNumber), Times.Once());
        }
    }
}
