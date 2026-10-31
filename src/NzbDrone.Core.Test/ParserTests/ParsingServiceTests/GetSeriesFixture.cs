using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetSeriesFixture : CoreTest<ParsingService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.FindAllByTitle(It.IsAny<string>()))
                  .Returns(new List<Series>());

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.FindAllByTitle(It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(new List<Series>());
        }

        [Test]
        public void should_use_passed_in_title_when_it_cannot_be_parsed()
        {
            const string title = "30 Stone";

            Subject.GetSeries(title);

            Mocker.GetMock<ISeriesService>()
                  .Verify(s => s.FindAllByTitle(title), Times.Once());
        }

        [Test]
        public void should_use_parsed_series_title()
        {
            const string title = "30.Stone.S01E01.720p.hdtv";

            Subject.GetSeries(title);

            Mocker.GetMock<ISeriesService>()
                  .Verify(s => s.FindAllByTitle(Parser.Parser.ParseTitle(title).SeriesTitle), Times.Once());
        }

        [Test]
        public void should_fallback_to_title_without_year_and_year_when_title_lookup_fails()
        {
            const string title = "Show.2004.S01E01.720p.hdtv";
            var parsedEpisodeInfo = Parser.Parser.ParseTitle(title);

            Subject.GetSeries(title);

            Mocker.GetMock<ISeriesService>()
                  .Verify(s => s.FindAllByTitle(parsedEpisodeInfo.SeriesTitleInfo.TitleWithoutYear,
                                             parsedEpisodeInfo.SeriesTitleInfo.Year),
                      Times.Once());
        }

        [Test]
        public void should_report_ambiguous_series_when_title_matches_multiple_series()
        {
            var series = new Series { Id = 1, Title = "30 Stone", TvdbId = 100 };
            var otherSeries = new Series { Id = 2, Title = "30 Stone", TvdbId = 200 };

            Mocker.GetMock<ISeriesService>().Setup(v => v.FindAllByTitle("30 Stone")).Returns(new List<Series> { series, otherSeries });

            var result = Subject.GetSeriesMatch("30.Stone.S01E01.720p.hdtv");

            result.Series.Should().BeNull();
            result.AmbiguousSeries.Should().BeEquivalentTo(new[] { series, otherSeries });
        }

        [Test]
        public void should_parse_concatenated_title()
        {
            var series = new Series { TvdbId = 100 };
            Mocker.GetMock<ISeriesService>().Setup(v => v.FindAllByTitle("Welcome")).Returns(new List<Series> { series });
            Mocker.GetMock<ISceneMappingService>().Setup(v => v.FindTvdbId("Mairimashita", It.IsAny<string>(), It.IsAny<int>())).Returns(100);

            var result = Subject.GetSeries("Welcome (Mairimashita).S01E01.720p.WEB-DL-Viva");

            result.Should().NotBeNull();
            result.TvdbId.Should().Be(100);
        }

        [Test]
        public void should_report_alias_match_type_when_matched_by_scene_mapping()
        {
            var series = new Series { Id = 1, TvdbId = 100 };

            Mocker.GetMock<ISceneMappingService>().Setup(v => v.FindTvdbId("30 Stone", It.IsAny<string>(), It.IsAny<int>())).Returns(100);
            Mocker.GetMock<ISeriesService>().Setup(v => v.FindByTvdbId(100)).Returns(series);

            var result = Subject.GetSeriesMatch("30.Stone.S01E01.720p.hdtv");

            result.Series.Should().Be(series);
            result.MatchType.Should().Be(SeriesMatchType.Alias);
        }

        [Test]
        public void should_report_title_match_type_when_matched_by_title()
        {
            var series = new Series { Id = 1, TvdbId = 100 };

            Mocker.GetMock<ISeriesService>().Setup(v => v.FindAllByTitle("30 Stone")).Returns(new List<Series> { series });

            var result = Subject.GetSeriesMatch("30.Stone.S01E01.720p.hdtv");

            result.Series.Should().Be(series);
            result.MatchType.Should().Be(SeriesMatchType.Title);
        }
    }
}
