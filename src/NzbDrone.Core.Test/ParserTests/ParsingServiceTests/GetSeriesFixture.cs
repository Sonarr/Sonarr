﻿using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using Workarr.DataAugmentation.Scene;
using Workarr.Parser;
using Workarr.Tv;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetSeriesFixture : CoreTest<ParsingService>
    {
        [Test]
        public void should_use_passed_in_title_when_it_cannot_be_parsed()
        {
            const string title = "30 Stone";

            Subject.GetSeries(title);

            Mocker.GetMock<ISeriesService>()
                  .Verify(s => s.FindByTitle(title), Times.Once());
        }

        [Test]
        public void should_use_parsed_series_title()
        {
            const string title = "30.Stone.S01E01.720p.hdtv";

            Subject.GetSeries(title);

            Mocker.GetMock<ISeriesService>()
                  .Verify(s => s.FindByTitle(Parser.ParseTitle(title).SeriesTitle), Times.Once());
        }

        [Test]
        public void should_fallback_to_title_without_year_and_year_when_title_lookup_fails()
        {
            const string title = "Show.2004.S01E01.720p.hdtv";
            var parsedEpisodeInfo = Parser.ParseTitle(title);

            Subject.GetSeries(title);

            Mocker.GetMock<ISeriesService>()
                  .Verify(s => s.FindByTitle(parsedEpisodeInfo.SeriesTitleInfo.TitleWithoutYear,
                                             parsedEpisodeInfo.SeriesTitleInfo.Year), Times.Once());
        }

        [Test]
        public void should_parse_concatenated_title()
        {
            var series = new Series { TvdbId = 100 };
            Mocker.GetMock<ISeriesService>().Setup(v => v.FindByTitle("Welcome")).Returns(series);
            Mocker.GetMock<ISceneMappingService>().Setup(v => v.FindTvdbId("Mairimashita", It.IsAny<string>(), It.IsAny<int>())).Returns(100);

            var result = Subject.GetSeries("Welcome (Mairimashita).S01E01.720p.WEB-DL-Viva");

            result.Should().NotBeNull();
            result.TvdbId.Should().Be(100);
        }
    }
}
