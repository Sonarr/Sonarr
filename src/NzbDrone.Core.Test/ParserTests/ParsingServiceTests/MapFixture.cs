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
                  .Setup(s => s.FindByTitle(It.IsAny<String>()))
                  .Returns(_series);
        }

        private void GivenMatchByTvRageId()
        {
            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.FindByTvRageId(It.IsAny<Int32>()))
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

            Subject.Map(_parsedEpisodeInfo, _series.TvRageId);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTitle(It.IsAny<String>()), Times.Once());
        }

        [Test]
        public void should_use_tvrageid_when_series_title_lookup_fails()
        {
            GivenMatchByTvRageId();

            Subject.Map(_parsedEpisodeInfo, _series.TvRageId);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTvRageId(It.IsAny<Int32>()), Times.Once());
        }

        [Test]
        public void should_use_search_criteria_series_title()
        {
            GivenMatchBySeriesTitle();

            Subject.Map(_parsedEpisodeInfo, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTitle(It.IsAny<String>()), Times.Never());
        }

        [Test]
        public void should_FindByTitle_when_search_criteria_matching_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedEpisodeInfo, 10, _singleEpisodeSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTitle(It.IsAny<String>()), Times.Once());
        }

        [Test]
        public void should_FindByTvRageId_when_search_criteria_and_FIndByTitle_matching_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedEpisodeInfo, 10, _singleEpisodeSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTvRageId(It.IsAny<Int32>()), Times.Once());
        }

        [Test]
        public void should_use_tvdbid_matching_when_alias_is_found()
        {
            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetTvDbId(It.IsAny<String>()))
                  .Returns(_series.TvdbId);

            Subject.Map(_parsedEpisodeInfo, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTitle(It.IsAny<String>()), Times.Never());
        }

        [Test]
        public void should_use_tvrageid_match_from_search_criteria_when_title_match_fails()
        {
            GivenParseResultSeriesDoesntMatchSearchCriteria();

            Subject.Map(_parsedEpisodeInfo, _series.TvRageId, _singleEpisodeSearchCriteria);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.FindByTitle(It.IsAny<String>()), Times.Never());
        }
    }
}
