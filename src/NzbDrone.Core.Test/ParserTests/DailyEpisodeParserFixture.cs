using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Expansive;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class DailyEpisodeParserFixture : CoreTest
    {
        [TestCase("Conan 2011 04 18 Emma Roberts HDTV XviD BFF", "Conan", 2011, 04, 18)]
        [TestCase("The Tonight Show With Jay Leno 2011 04 15 1080i HDTV DD5 1 MPEG2 TrollHD", "The Tonight Show With Jay Leno", 2011, 04, 15)]
        [TestCase("The.Daily.Show.2010.10.11.Johnny.Knoxville.iTouch-MW", "The Daily Show", 2010, 10, 11)]
        [TestCase("The Daily Show - 2011-04-12 - Gov. Deval Patrick", "The Daily Show", 2011, 04, 12)]
        [TestCase("2011.01.10 - Denis Leary - HD TV.mkv", "", 2011, 1, 10)]
        [TestCase("2011.03.13 - Denis Leary - HD TV.mkv", "", 2011, 3, 13)]
        [TestCase("The Tonight Show with Jay Leno - 2011-06-16 - Larry David, \"Bachelorette\" Ashley Hebert, Pitbull with Ne-Yo", "The Tonight Show with Jay Leno", 2011, 6, 16)]
        [TestCase("2020.NZ.2012.16.02.PDTV.XviD-C4TV", "2020 NZ", 2012, 2, 16)]
        [TestCase("2020.NZ.2012.13.02.PDTV.XviD-C4TV", "2020 NZ", 2012, 2, 13)]
        [TestCase("2020.NZ.2011.12.02.PDTV.XviD-C4TV", "2020 NZ", 2011, 12, 2)]
        [TestCase("Series Title - 2013-10-30 - Episode Title (1) [HDTV-720p]", "Series Title", 2013, 10, 30)]
        [TestCase("The_Voice_US_04.28.2014_hdtv.x264.Poke.mp4", "The Voice US", 2014, 4, 28)]
        [TestCase("At.Midnight.140722.720p.HDTV.x264-YesTV", "At Midnight", 2014, 07, 22)]
        [TestCase("At_Midnight_140722_720p_HDTV_x264-YesTV", "At Midnight", 2014, 07, 22)]
        //[TestCase("Corrie.07.01.15", "Corrie", 2015, 1, 7)]
        [TestCase("The Nightly Show with Larry Wilmore 2015 02 09 WEBRIP s01e13", "The Nightly Show with Larry Wilmore", 2015, 2, 9)]
        //[TestCase("", "", 0, 0, 0)]
        public void should_parse_daily_episode(string postTitle, string title, int year, int month, int day)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            var airDate = new DateTime(year, month, day);
            result.Should().NotBeNull();
            result.SeriesTitle.Should().Be(title);
            result.AirDate.Should().Be(airDate.ToString(Episode.AIR_DATE_FORMAT));
            result.EpisodeNumbers.Should().BeEmpty();
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();
        }

        [TestCase("Conan {year} {month} {day} Emma Roberts HDTV XviD BFF")]
        [TestCase("The Tonight Show With Jay Leno {year} {month} {day} 1080i HDTV DD5 1 MPEG2 TrollHD")]
        [TestCase("The.Daily.Show.{year}.{month}.{day}.Johnny.Knoxville.iTouch-MW")]
        [TestCase("The Daily Show - {year}-{month}-{day} - Gov. Deval Patrick")]
        [TestCase("{year}.{month}.{day} - Denis Leary - HD TV.mkv")]
        [TestCase("The Tonight Show with Jay Leno - {year}-{month}-{day} - Larry David, \"Bachelorette\" Ashley Hebert, Pitbull with Ne-Yo")]
        [TestCase("2020.NZ.{year}.{month}.{day}.PDTV.XviD-C4TV")]
        public void should_not_accept_ancient_daily_series(string title)
        {
            var yearTooLow = title.Expand(new { year = 1950, month = 10, day = 14 });
            Parser.Parser.ParseTitle(yearTooLow).Should().BeNull();
        }

        [TestCase("Conan {year} {month} {day} Emma Roberts HDTV XviD BFF")]
        [TestCase("The Tonight Show With Jay Leno {year} {month} {day} 1080i HDTV DD5 1 MPEG2 TrollHD")]
        [TestCase("The.Daily.Show.{year}.{month}.{day}.Johnny.Knoxville.iTouch-MW")]
        [TestCase("The Daily Show - {year}-{month}-{day} - Gov. Deval Patrick")]
        [TestCase("{year}.{month}.{day} - Denis Leary - HD TV.mkv")]
        [TestCase("The Tonight Show with Jay Leno - {year}-{month}-{day} - Larry David, \"Bachelorette\" Ashley Hebert, Pitbull with Ne-Yo")]
        [TestCase("2020.NZ.{year}.{month}.{day}.PDTV.XviD-C4TV")]
        public void should_not_accept_future_dates(string title)
        {
            var twoDaysFromNow = DateTime.Now.AddDays(2);

            var validDate = title.Expand(new { year = twoDaysFromNow.Year, month = twoDaysFromNow.Month.ToString("00"), day = twoDaysFromNow.Day.ToString("00") });

            Parser.Parser.ParseTitle(validDate).Should().BeNull();
        }

        [Test]
        public void should_fail_if_episode_is_far_in_future()
        {
            var title = string.Format("{0:yyyy.MM.dd} - Denis Leary - HD TV.mkv", DateTime.Now.AddDays(2));

            Parser.Parser.ParseTitle(title).Should().BeNull();
        }
    }
}
