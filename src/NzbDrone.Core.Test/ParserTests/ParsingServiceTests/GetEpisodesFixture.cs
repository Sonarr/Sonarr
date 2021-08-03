using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetEpisodesFixture : TestBase<ParsingService>
    {
        private Series _series;
        private List<Episode> _episodes;
        private ParsedEpisodeInfo _parsedEpisodeInfo;
        private SingleEpisodeSearchCriteria _singleEpisodeSearchCriteria;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                .With(s => s.Title = "30 Stone")
                .With(s => s.CleanTitle = "stone")
                .Build();

            _episodes = Builder<Episode>.CreateListOfSize(1)
                                        .All()
                                        .With(e => e.AirDate = DateTime.Today.ToString(Episode.AIR_DATE_FORMAT))
                                        .Build()
                                        .ToList();

            _parsedEpisodeInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = _series.Title,
                SeasonNumber = 1,
                EpisodeNumbers = new[] { 1 },
                AbsoluteEpisodeNumbers = new int[0]
            };

            _singleEpisodeSearchCriteria = new SingleEpisodeSearchCriteria
            {
                Series = _series,
                EpisodeNumber = _episodes.First().EpisodeNumber,
                SeasonNumber = _episodes.First().SeasonNumber,
                Episodes = _episodes
            };

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.FindByTitle(It.IsAny<string>()))
                  .Returns(_series);
        }

        private void GivenDailySeries()
        {
            _series.SeriesType = SeriesTypes.Daily;
        }

        private void GivenDailyParseResult()
        {
            _parsedEpisodeInfo.AirDate = DateTime.Today.ToString(Episode.AIR_DATE_FORMAT);
        }

        private void GivenSceneNumberingSeries()
        {
            _series.UseSceneNumbering = true;
        }

        private void GivenAbsoluteNumberingSeries()
        {
            _parsedEpisodeInfo.AbsoluteEpisodeNumbers = new[] { 1 };
        }

        private void GivenFullSeason()
        {
            _parsedEpisodeInfo.FullSeason = true;
            _parsedEpisodeInfo.EpisodeNumbers = Array.Empty<int>();
        }

        [Test]
        public void should_get_daily_episode_episode_when_search_criteria_is_null()
        {
            GivenDailySeries();
            GivenDailyParseResult();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>(), null), Times.Once());
        }

        [Test]
        public void should_use_search_criteria_episode_when_it_matches_daily()
        {
            GivenDailySeries();
            GivenDailyParseResult();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId,  _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>(), null), Times.Never());
        }

        [Test]
        public void should_fallback_to_daily_episode_lookup_when_search_criteria_episode_doesnt_match()
        {
            GivenDailySeries();
            _parsedEpisodeInfo.AirDate = DateTime.Today.AddDays(-5).ToString(Episode.AIR_DATE_FORMAT);

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>(), null), Times.Once());
        }

        [Test]
        public void should_get_daily_episode_episode_should_lookup_including_daily_part()
        {
            GivenDailySeries();
            GivenDailyParseResult();
            _parsedEpisodeInfo.DailyPart = 1;

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>(), 1), Times.Once());
        }

        [Test]
        public void should_use_search_criteria_episode_when_it_matches_absolute()
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(new List<Episode>());

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>(), null), Times.Never());
        }

        [Test]
        public void should_use_scene_numbering_when_series_uses_scene_numbering()
        {
            GivenSceneNumberingSeries();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_match_search_criteria_by_scene_numbering()
        {
            GivenSceneNumberingSeries();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_fallback_to_findEpisode_when_search_criteria_match_fails_for_scene_numbering()
        {
            GivenSceneNumberingSeries();
            _episodes.First().SceneEpisodeNumber = 10;

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_find_episode()
        {
            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_match_episode_with_search_criteria()
        {
            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_fallback_to_findEpisode_when_search_criteria_match_fails()
        {
            _episodes.First().EpisodeNumber = 10;

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_look_for_episode_in_season_zero_if_absolute_special()
        {
            GivenAbsoluteNumberingSeries();

            _parsedEpisodeInfo.Special = true;

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), 0, It.IsAny<int>()), Times.Never());

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), 0, It.IsAny<int>()), Times.Once());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_use_scene_numbering_when_scene_season_number_has_value(int seasonNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetSceneSeasonNumber(_parsedEpisodeInfo.SeriesTitle, It.IsAny<string>()))
                  .Returns(seasonNumber);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()))
                  .Returns(new List<Episode>());

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Once());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_find_episode_by_season_and_scene_absolute_episode_number(int seasonNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetSceneSeasonNumber(_parsedEpisodeInfo.SeriesTitle, It.IsAny<string>()))
                  .Returns(seasonNumber);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()))
                  .Returns(new List<Episode> { _episodes.First() });

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Never());
        }

        [TestCase(2)]
        [TestCase(20)]
        public void should_find_episode_by_parsed_season_and_absolute_episode_number_when_season_number_is_2_or_higher(int seasonNumber)
        {
            GivenAbsoluteNumberingSeries();
            _parsedEpisodeInfo.SeasonNumber = seasonNumber;
            _parsedEpisodeInfo.EpisodeNumbers = new int[] { };

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()))
                  .Returns(new List<Episode> { _episodes.First() });

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Never());
        }

        [TestCase(2)]
        [TestCase(20)]
        public void should_find_episode_by_parsed_season_and_absolute_episode_number_when_season_number_is_2_or_higher_and_scene_season_number_lookup_failed(int seasonNumber)
        {
            GivenAbsoluteNumberingSeries();
            _parsedEpisodeInfo.SeasonNumber = seasonNumber;
            _parsedEpisodeInfo.EpisodeNumbers = new int[] { };

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()))
                  .Returns(new List<Episode>());

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisode(It.IsAny<int>(), seasonNumber, It.IsAny<int>()))
                  .Returns(_episodes.First());

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Once());
        }

        [TestCase(2)]
        [TestCase(20)]
        public void should_not_find_episode_by_parsed_season_and_absolute_episode_number_when_season_number_is_2_or_higher_and_a_episode_number_was_parsed(int seasonNumber)
        {
            GivenAbsoluteNumberingSeries();
            _parsedEpisodeInfo.SeasonNumber = seasonNumber;
            _parsedEpisodeInfo.EpisodeNumbers = new[] { 1 };

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(new List<Episode> { _episodes.First() });

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Never());

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Never());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_return_episodes_when_scene_absolute_episode_number_returns_multiple_results(int seasonNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetSceneSeasonNumber(_parsedEpisodeInfo.SeriesTitle, It.IsAny<string>()))
                  .Returns(seasonNumber);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()))
                  .Returns(Builder<Episode>.CreateListOfSize(5).Build().ToList());

            var result = Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            result.Should().HaveCount(5);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Never());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_find_episode_by_season_and_absolute_episode_number_when_scene_absolute_episode_number_returns_no_results(int seasonNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetSceneSeasonNumber(_parsedEpisodeInfo.SeriesTitle, It.IsAny<string>()))
                  .Returns(seasonNumber);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()))
                  .Returns(new List<Episode>());

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_use_tvdb_season_number_when_available_and_a_scene_source()
        {
            const int tvdbSeasonNumber = 5;

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(v => v.FindSceneMapping(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns<string, string, int>((s, r, sn) => new SceneMapping { SceneSeasonNumber = 1, SeasonNumber = tvdbSeasonNumber });

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(_series.Id, _parsedEpisodeInfo.SeasonNumber, _parsedEpisodeInfo.EpisodeNumbers.First()), Times.Never());

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(_series.Id, tvdbSeasonNumber, _parsedEpisodeInfo.EpisodeNumbers.First()), Times.Once());
        }

        [Test]
        public void should_not_use_tvdb_season_number_when_available_for_a_different_season_and_a_scene_source()
        {
            const int tvdbSeasonNumber = 5;

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(v => v.FindSceneMapping(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns<string, string, int>((s, r, sn) => new SceneMapping { SceneSeasonNumber = 101, SeasonNumber = tvdbSeasonNumber });

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(_series.Id, tvdbSeasonNumber, _parsedEpisodeInfo.EpisodeNumbers.First()), Times.Never());

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(_series.Id, _parsedEpisodeInfo.SeasonNumber, _parsedEpisodeInfo.EpisodeNumbers.First()), Times.Once());
        }

        [Test]
        public void should_not_use_tvdb_season_when_not_a_scene_source()
        {
            const int tvdbSeasonNumber = 5;

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, false, null);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(_series.Id, tvdbSeasonNumber, _parsedEpisodeInfo.EpisodeNumbers.First()), Times.Never());

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(_series.Id, _parsedEpisodeInfo.SeasonNumber, _parsedEpisodeInfo.EpisodeNumbers.First()), Times.Once());
        }

        [Test]
        public void should_not_use_tvdb_season_when_tvdb_season_number_is_less_than_zero()
        {
            const int tvdbSeasonNumber = -1;

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.FindSceneMapping(_parsedEpisodeInfo.SeriesTitle, It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(new SceneMapping { SeasonNumber = tvdbSeasonNumber, SceneSeasonNumber = _parsedEpisodeInfo.SeasonNumber });

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(_series.Id, tvdbSeasonNumber, _parsedEpisodeInfo.EpisodeNumbers.First()), Times.Never());

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(_series.Id, _parsedEpisodeInfo.SeasonNumber, _parsedEpisodeInfo.EpisodeNumbers.First()), Times.Once());
        }

        [Test]
        public void should_lookup_full_season_by_season_number_if_series_does_not_use_scene_numbering()
        {
            GivenFullSeason();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodesBySeason(_series.Id, _parsedEpisodeInfo.SeasonNumber))
                .Returns(_episodes);

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.GetEpisodesBySceneSeason(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void should_lookup_full_season_by_scene_season_number_if_series_uses_scene_numbering()
        {
            GivenSceneNumberingSeries();
            GivenFullSeason();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodesBySceneSeason(_series.Id, _parsedEpisodeInfo.SeasonNumber))
                .Returns(_episodes);

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.GetEpisodesBySceneSeason(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void should_fallback_to_lookup_full_season_by_season_number_if_series_uses_scene_numbering_and_no_epsiodes_are_found_by_scene_season_number()
        {
            GivenSceneNumberingSeries();
            GivenFullSeason();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodesBySceneSeason(_series.Id, _parsedEpisodeInfo.SeasonNumber))
                .Returns(new List<Episode>());

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodesBySeason(_series.Id, _parsedEpisodeInfo.SeasonNumber))
                .Returns(_episodes);

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.GetEpisodesBySceneSeason(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void should_use_season_zero_when_looking_up_is_partial_special_episode_found_by_title()
        {
            _series.UseSceneNumbering = false;
            _parsedEpisodeInfo.SeasonNumber = 1;
            _parsedEpisodeInfo.EpisodeNumbers = new int[] { 0 };
            _parsedEpisodeInfo.ReleaseTitle = "Series.Title.S01E00.My.Special.Episode.1080p.AMZN.WEB-DL.DDP5.1.H264-TEPES";

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodeByTitle(_series.TvdbId, 0, _parsedEpisodeInfo.ReleaseTitle))
                  .Returns(
                      Builder<Episode>.CreateNew()
                                      .With(e => e.SeasonNumber = 0)
                                      .With(e => e.EpisodeNumber = 1)
                                      .Build());

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisode(_series.TvdbId, 0, 1), Times.Once());
        }

        [Test]
        public void should_use_original_parse_result_when_special_episode_lookup_by_title_fails()
        {
            _series.UseSceneNumbering = false;
            _parsedEpisodeInfo.SeasonNumber = 1;
            _parsedEpisodeInfo.EpisodeNumbers = new int[] { 0 };
            _parsedEpisodeInfo.ReleaseTitle = "Series.Title.S01E00.My.Special.Episode.1080p.AMZN.WEB-DL.DDP5.1.H264-TEPES";

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodeByTitle(_series.TvdbId, 0, _parsedEpisodeInfo.ReleaseTitle))
                  .Returns((Episode)null);

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisode(_series.TvdbId, _parsedEpisodeInfo.SeasonNumber, _parsedEpisodeInfo.EpisodeNumbers.First()), Times.Once());
        }
    }
}
