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
    public class MapFixture : TestBase<ParsingService>
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
                SeriesTitleInfo = new SeriesTitleInfo(),
                SeasonNumber = 1,
                EpisodeNumbers = new[] { 1 }
            };

            _singleEpisodeSearchCriteria = new SingleEpisodeSearchCriteria
            {
                Series = _series,
                EpisodeNumber = _episodes.First().EpisodeNumber,
                SeasonNumber = _episodes.First().SeasonNumber,
                Episodes = _episodes
            };
        }

        private void GivenMatchBySeriesTitle()
        {
            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.FindByTitle(It.IsAny<string>()))
                  .Returns(_series);
        }

        private void GivenMatchByTvdbId()
        {
            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.FindByTvdbId(It.IsAny<int>()))
                  .Returns(_series);
        }

        private void GivenMatchByTvRageId()
        {
            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.FindByTvRageId(It.IsAny<int>()))
                  .Returns(_series);
        }

        private void GivenParseResultSeriesDoesntMatchSearchCriteria()
        {
            _parsedEpisodeInfo.SeriesTitle = "Another Name";
        }

        [Test]
        public void should_lookup_series_by_name()
        {
            GivenMatchBySeriesTitle();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_use_tvdbid_when_series_title_lookup_fails()
        {
            GivenMatchByTvdbId();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTvdbId(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_use_tvrageid_when_series_title_lookup_fails()
        {
            GivenMatchByTvRageId();

            Subject.Map(_parsedEpisodeInfo, 0, _series.TvRageId);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTvRageId(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_not_use_tvrageid_when_scene_naming_exception_exists()
        {
            GivenMatchByTvRageId();

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(v => v.FindSceneMapping(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(new SceneMapping { TvdbId = 10 });

            var result = Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTvRageId(It.IsAny<int>()), Times.Never());

            result.Series.Should().BeNull();
        }

        [Test]
        public void should_use_search_criteria_series_title()
        {
            GivenMatchBySeriesTitle();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_FindByTitle_when_search_criteria_matching_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedEpisodeInfo, 10, 10, _singleEpisodeSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_FindByTitle_using_year_when_FindByTitle_matching_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            _parsedEpisodeInfo.SeriesTitleInfo = new SeriesTitleInfo
            {
                Title = "Series Title 2017",
                TitleWithoutYear = "Series Title",
                Year = 2017
            };

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.FindByTitle(_parsedEpisodeInfo.SeriesTitleInfo.TitleWithoutYear, _parsedEpisodeInfo.SeriesTitleInfo.Year))
                  .Returns(_series);

            Subject.Map(_parsedEpisodeInfo, 10, 10, _singleEpisodeSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_FindByTvdbId_when_search_criteria_and_FindByTitle_matching_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedEpisodeInfo, 10, 10, _singleEpisodeSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTvdbId(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_FindByTvRageId_when_search_criteria_and_FindByTitle_matching_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedEpisodeInfo, 10, 10, _singleEpisodeSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTvRageId(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void should_use_tvdbid_matching_when_alias_is_found()
        {
            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.FindTvdbId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(_series.TvdbId);

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_tvrageid_match_from_search_criteria_when_title_match_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedEpisodeInfo, _series.TvdbId, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTitle(It.IsAny<string>()), Times.Never());
        }
    }
}
