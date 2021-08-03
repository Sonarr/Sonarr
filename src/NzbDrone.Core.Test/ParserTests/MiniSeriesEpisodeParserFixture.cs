using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class MiniSeriesEpisodeParserFixture : CoreTest
    {
        [TestCase("The.Big.Series.Leader.Part.2.DSR.XviD-SYS", "The Big Series Leader", 2)]
        [TestCase("kill-roy-was-here-e07-720p", "kill-roy-was-here", 7)]
        [TestCase("Series and Show 2012 Part 1 REPACK 720p HDTV x264 2HD", "Series and Show 2012", 1)]
        [TestCase("Series Show.2016.E04.Power.720p.WEB-DL.DD5.1.H.264-MARS", "Series Show 2016", 4)]

        //[TestCase("Killroy.Jumped.And.Was.Here.EP02.Episode.Title.DVDRiP.XviD-DEiTY", "Killroy.Jumped.And.Was.Here", 2)]
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

        [TestCase("It's a Series Title.E56.190121.720p-NEXT.mp4", "It's a Series Title", 56, "2019-01-21")]
        [TestCase("My Only Series Title.E37.190120.1080p-NEXT.mp4", "My Only Series Title", 37, "2019-01-20")]
        [TestCase("Series.E191.190121.720p-NEXT.mp4", "Series", 191, "2019-01-21")]
        [TestCase("The Series Title Challenge.E932.190120.720p-NEXT.mp4", "The Series Title Challenge", 932, "2019-01-20")]

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
