using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerSearchTests.EpisodeSearchTests
{
    [TestFixture]
    public class IndexerEpisodeSearch_EpisodeMatch : TestBase
    {
        private Series _series;
        private Episode _episode;
        private EpisodeParseResult _episodeParseResult;
            
        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .Build();

            _episode = Builder<Episode>
                    .CreateNew()
                    .With(e => e.SeriesId = _series.Id)
                    .With(e => e.Series = _series)
                    .Build();

            _episodeParseResult = Builder<EpisodeParseResult>
                    .CreateNew()
                    .With(p => p.SeasonNumber = 1)
                    .With(p => p.EpisodeNumbers = new List<int>{ _episode.EpisodeNumber })
                    .With(p => p.Episodes = new List<Episode> { _episode })
                    .With(p => p.Series = _series)
                    .Build();

        }

        [Test]
        public void should_return_WrongSeason_when_season_doesnt_match()
        {
            _episode.SeasonNumber = 10;

            Mocker.Resolve<EpisodeSearch>()
                  .IsEpisodeMatch(_series, new {Episode = _episode}, _episodeParseResult)
                  .Should()
                  .BeFalse();
        }

        [Test]
        public void should_return_WrongEpisode_when_episode_doesnt_match()
        {
            _episode.EpisodeNumber = 10;

            Mocker.Resolve<EpisodeSearch>()
                  .IsEpisodeMatch(_series, new { Episode = _episode }, _episodeParseResult)
                  .Should()
                  .BeFalse();
        }

        [Test]
        public void should_not_return_error_when_season_and_episode_match()
        {
            Mocker.Resolve<EpisodeSearch>()
                  .IsEpisodeMatch(_series, new { Episode = _episode }, _episodeParseResult)
                  .Should()
                  .BeTrue();
        }

        [Test]
        public void should_return_WrongSeason_when_season_doesnt_match_for_scene_series()
        {
            _series.UseSceneNumbering = true;
            _episode.SceneSeasonNumber = 10;
            _episode.SeasonNumber = 10;
            _episode.EpisodeNumber = 10;

            Mocker.Resolve<EpisodeSearch>()
                  .IsEpisodeMatch(_series, new { Episode = _episode }, _episodeParseResult)
                 .Should()
                  .BeFalse();
        }

        [Test]
        public void should_return_WrongEpisode_when_episode_doesnt_match_for_scene_series()
        {
            _series.UseSceneNumbering = true;
            _episode.SceneEpisodeNumber = 10;
            _episode.SeasonNumber = 10;
            _episode.EpisodeNumber = 10;

            Mocker.Resolve<EpisodeSearch>()
                  .IsEpisodeMatch(_series, new { Episode = _episode }, _episodeParseResult)
                 .Should()
                  .BeFalse();
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
                  .IsEpisodeMatch(_series, new { Episode = _episode }, _episodeParseResult)
                  .Should()
                  .BeTrue();
        }
    }
}
