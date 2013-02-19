using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Providers.Search;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Search;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SearchTests.EpisodeSearchTests
{
    [TestFixture]
    public class CheckReportFixture : TestBase
    {
        private Series _series;
        private Episode _episode;
        private EpisodeParseResult _episodeParseResult;
        private SearchHistoryItem _searchHistoryItem;
            
        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .Build();

            _episode = Builder<Episode>
                    .CreateNew()
                    .With(e => e.SeriesId = _series.SeriesId)
                    .With(e => e.Series = _series)
                    .Build();

            _episodeParseResult = Builder<EpisodeParseResult>
                    .CreateNew()
                    .With(p => p.SeasonNumber = 1)
                    .With(p => p.EpisodeNumbers = new List<int>{ _episode.EpisodeNumber })
                    .With(p => p.Episodes = new List<Episode> { _episode })
                    .With(p => p.Series = _series)
                    .Build();

            _searchHistoryItem = new SearchHistoryItem();
        }

        [Test]
        public void should_return_WrongSeason_when_season_doesnt_match()
        {
            _episode.SeasonNumber = 10;

            Mocker.Resolve<EpisodeSearch>()
                  .CheckReport(_series, new {Episode = _episode}, _episodeParseResult, _searchHistoryItem)
                  .SearchError
                  .Should()
                  .Be(ReportRejectionType.WrongSeason);
        }

        [Test]
        public void should_return_WrongEpisode_when_episode_doesnt_match()
        {
            _episode.EpisodeNumber = 10;

            Mocker.Resolve<EpisodeSearch>()
                  .CheckReport(_series, new { Episode = _episode }, _episodeParseResult, _searchHistoryItem)
                  .SearchError
                  .Should()
                  .Be(ReportRejectionType.WrongEpisode);
        }

        [Test]
        public void should_not_return_error_when_season_and_episode_match()
        {
            Mocker.Resolve<EpisodeSearch>()
                  .CheckReport(_series, new { Episode = _episode }, _episodeParseResult, _searchHistoryItem)
                  .SearchError
                  .Should()
                  .Be(ReportRejectionType.None);
        }

        [Test]
        public void should_return_WrongSeason_when_season_doesnt_match_for_scene_series()
        {
            _series.UseSceneNumbering = true;
            _episode.SceneSeasonNumber = 10;
            _episode.SeasonNumber = 10;
            _episode.EpisodeNumber = 10;

            Mocker.Resolve<EpisodeSearch>()
                  .CheckReport(_series, new { Episode = _episode }, _episodeParseResult, _searchHistoryItem)
                  .SearchError
                  .Should()
                  .Be(ReportRejectionType.WrongSeason);
        }

        [Test]
        public void should_return_WrongEpisode_when_episode_doesnt_match_for_scene_series()
        {
            _series.UseSceneNumbering = true;
            _episode.SceneEpisodeNumber = 10;
            _episode.SeasonNumber = 10;
            _episode.EpisodeNumber = 10;

            Mocker.Resolve<EpisodeSearch>()
                  .CheckReport(_series, new { Episode = _episode }, _episodeParseResult, _searchHistoryItem)
                  .SearchError
                  .Should()
                  .Be(ReportRejectionType.WrongEpisode);
        }

        [Test]
        public void should_not_return_error_when_season_and_episode_match_for_scene_series()
        {
            _series.UseSceneNumbering = true;
            _episode.SceneSeasonNumber = _episode.SeasonNumber;
            _episode.SceneEpisodeNumber = _episode.EpisodeNumber;
            _episode.SeasonNumber = 10;
            _episode.EpisodeNumber = 10;

            Mocker.Resolve<EpisodeSearch>()
                  .CheckReport(_series, new { Episode = _episode }, _episodeParseResult, _searchHistoryItem)
                  .SearchError
                  .Should()
                  .Be(ReportRejectionType.None);
        }
    }
}
