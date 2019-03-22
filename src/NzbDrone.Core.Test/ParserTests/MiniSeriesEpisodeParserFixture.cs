using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class MiniSeriesEpisodeParserFixture : CoreTest
    {
        [TestCase("The.Kennedys.Part.2.DSR.XviD-SYS", "The Kennedys", 2)]
        [TestCase("the-pacific-e07-720p", "the-pacific", 7)]
        [TestCase("Hatfields and McCoys 2012 Part 1 REPACK 720p HDTV x264 2HD", "Hatfields and McCoys 2012", 1)]
        [TestCase("Mars.2016.E04.Power.720p.WEB-DL.DD5.1.H.264-MARS", "Mars 2016", 4)]
        //[TestCase("Band.Of.Brothers.EP02.Day.Of.Days.DVDRiP.XviD-DEiTY", "Band.Of.Brothers", 2)]
        //[TestCase("", "", 0)]
        public void should_parse_mini_series_episode(string postTitle, string title, int episodeNumber)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.EpisodeNumbers.Should().HaveCount(1);
            result.SeasonNumber.Should().Be(1);
            result.EpisodeNumbers.First().Should().Be(episodeNumber);
            result.SeriesTitle.Should().Be(title);
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();
        }

        [TestCase("It's My Life.E56.190121.720p-NEXT.mp4", "It's My Life", 56, "2019-01-21")]
        [TestCase("My Only One.E37.190120.1080p-NEXT.mp4", "My Only One", 37, "2019-01-20")]
        [TestCase("Man.E191.190121.720p-NEXT.mp4", "Man",191, "2019-01-21")]
        [TestCase("The Golden Bell Challenge.E932.190120.720p-NEXT.mp4", "The Golden Bell Challenge", 932, "2019-01-20")]
        //[TestCase("", "", 0, "")]
        public void should_parse_korean_series_episode(string postTitle, string title, int episodeNumber, string airdate)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.EpisodeNumbers.Should().HaveCount(1);
            result.SeasonNumber.Should().Be(1);
            result.EpisodeNumbers.First().Should().Be(episodeNumber);
            result.SeriesTitle.Should().Be(title);
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();
            // We don't support both SxxExx and airdate yet
            //result.AirDate.Should().Be(airdate);
        }
    }
}
