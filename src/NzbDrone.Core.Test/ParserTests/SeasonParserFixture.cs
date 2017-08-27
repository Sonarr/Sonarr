using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class SeasonParserFixture : CoreTest
    {
        [TestCase("30.Rock.Season.04.HDTV.XviD-DIMENSION", "30 Rock", 4)]
        [TestCase("Parks.and.Recreation.S02.720p.x264-DIMENSION", "Parks and Recreation", 2)]
        [TestCase("The.Office.US.S03.720p.x264-DIMENSION", "The Office US", 3)]
        [TestCase(@"Sons.of.Anarchy.S03.720p.BluRay-CLUE\REWARD", "Sons of Anarchy", 3)]
        [TestCase("Adventure Time S02 720p HDTV x264 CRON", "Adventure Time", 2)]
        [TestCase("Sealab.2021.S04.iNTERNAL.DVDRip.XviD-VCDVaULT", "Sealab 2021", 4)]
        [TestCase("Hawaii Five 0 S01 720p WEB DL DD5 1 H 264 NT", "Hawaii Five 0", 1)]
        [TestCase("30 Rock S03 WS PDTV XviD FUtV", "30 Rock", 3)]
        [TestCase("The Office Season 4 WS PDTV XviD FUtV", "The Office", 4)]
        [TestCase("Eureka Season 1 720p WEB DL DD 5 1 h264 TjHD", "Eureka", 1)]
        [TestCase("The Office Season4 WS PDTV XviD FUtV", "The Office", 4)]
        [TestCase("Eureka S 01 720p WEB DL DD 5 1 h264 TjHD", "Eureka", 1)]
        [TestCase("Doctor Who Confidential   Season 3", "Doctor Who Confidential", 3)]
        [TestCase("Fleming.S01.720p.WEBDL.DD5.1.H.264-NTb", "Fleming", 1)]
        [TestCase("Holmes.Makes.It.Right.S02.720p.HDTV.AAC5.1.x265-NOGRP", "Holmes Makes It Right", 2)]
        [TestCase("My.Series.S2014.720p.HDTV.x264-ME", "My Series", 2014)]
        public void should_parse_full_season_release(string postTitle, string title, int season)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(season);
            result.SeriesTitle.Should().Be(title);
            result.EpisodeNumbers.Should().BeEmpty();
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeTrue();
        }

        [TestCase("Acropolis Now S05 EXTRAS DVDRip XviD RUNNER")]
        [TestCase("Punky Brewster S01 EXTRAS DVDRip XviD RUNNER")]
        [TestCase("Instant Star S03 EXTRAS DVDRip XviD OSiTV")]
        public void should_parse_season_extras(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);

            result.Should().BeNull();
        }

        [TestCase("Lie.to.Me.S03.SUBPACK.DVDRip.XviD-REWARD")]
        [TestCase("The.Middle.S02.SUBPACK.DVDRip.XviD-REWARD")]
        [TestCase("CSI.S11.SUBPACK.DVDRip.XviD-REWARD")]
        public void should_parse_season_subpack(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);

            result.Should().BeNull();
        }

        [TestCase("The.Ranch.2016.S02.Part.1.1080p.NF.WEBRip.DD5.1.x264-NTb", "The Ranch 2016", 2, 1)]
        public void should_parse_partial_season_release(string postTitle, string title, int season, int seasonPart)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(season);
            result.SeriesTitle.Should().Be(title);
            result.EpisodeNumbers.Should().BeEmpty();
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();
            result.IsPartialSeason.Should().BeTrue();
            result.SeasonPart.Should().Be(seasonPart);
        }
    }
}
