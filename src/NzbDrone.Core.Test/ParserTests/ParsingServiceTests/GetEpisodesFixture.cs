using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
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
                .With(s => s.Title = "30 Rock")
                .With(s => s.CleanTitle = "rock")
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

        [Test]
        public void should_get_daily_episode_episode_when_search_criteria_is_null()
        {
            GivenDailySeries();
            GivenDailyParseResult();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_use_search_criteria_episode_when_it_matches_daily()
        {
            GivenDailySeries();
            GivenDailyParseResult();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId,  _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_fallback_to_daily_episode_lookup_when_search_criteria_episode_doesnt_match()
        {
            GivenDailySeries();
            _parsedEpisodeInfo.AirDate = DateTime.Today.AddDays(-5).ToString(Episode.AIR_DATE_FORMAT); ;

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_use_search_criteria_episode_when_it_matches_absolute()
        {
            GivenAbsoluteNumberingSeries();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.FindEpisode(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
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
                  .Setup(s => s.GetSceneSeasonNumber(_parsedEpisodeInfo.SeriesTitle))
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
                  .Setup(s => s.GetSceneSeasonNumber(_parsedEpisodeInfo.SeriesTitle))
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

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void should_find_episode_by_season_and_absolute_episode_number_when_scene_absolute_episode_number_returns_multiple_results(int seasonNumber)
        {
            GivenAbsoluteNumberingSeries();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetSceneSeasonNumber(_parsedEpisodeInfo.SeriesTitle))
                  .Returns(seasonNumber);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()))
                  .Returns(Builder<Episode>.CreateListOfSize(5).Build().ToList());

            Subject.GetEpisodes(_parsedEpisodeInfo, _series, true, null);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisodesBySceneNumbering(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Once());

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.FindEpisode(It.IsAny<int>(), seasonNumber, It.IsAny<int>()), Times.Once());
        }
    }
}
