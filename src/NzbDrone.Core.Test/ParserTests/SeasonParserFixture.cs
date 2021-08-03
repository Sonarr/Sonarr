using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class SeasonParserFixture : CoreTest
    {
        [TestCase("30.Series.Season.04.HDTV.XviD-DIMENSION", "30 Series", 4)]
        [TestCase("Sonarr.and.Series.S02.720p.x264-DIMENSION", "Sonarr and Series", 2)]
        [TestCase("The.Series.US.S03.720p.x264-DIMENSION", "The Series US", 3)]
        [TestCase(@"Series.of.Sonarr.S03.720p.BluRay-CLUE\REWARD", "Series of Sonarr", 3)]
        [TestCase("Series Time S02 720p HDTV x264 CRON", "Series Time", 2)]
        [TestCase("Series.2021.S04.iNTERNAL.DVDRip.XviD-VCDVaULT", "Series 2021", 4)]
        [TestCase("Series Five 0 S01 720p WEB DL DD5 1 H 264 NT", "Series Five 0", 1)]
        [TestCase("30 Series S03 WS PDTV XviD FUtV", "30 Series", 3)]
        [TestCase("The Series Season 4 WS PDTV XviD FUtV", "The Series", 4)]
        [TestCase("Series Season 1 720p WEB DL DD 5 1 h264 TjHD", "Series", 1)]
        [TestCase("The Series Season4 WS PDTV XviD FUtV", "The Series", 4)]
        [TestCase("Series S 01 720p WEB DL DD 5 1 h264 TjHD", "Series", 1)]
        [TestCase("Series Confidential   Season 3", "Series Confidential", 3)]
        [TestCase("Series.S01.720p.WEBDL.DD5.1.H.264-NTb", "Series", 1)]
        [TestCase("Series.Makes.It.Right.S02.720p.HDTV.AAC5.1.x265-NOGRP", "Series Makes It Right", 2)]
        [TestCase("My.Series.S2014.720p.HDTV.x264-ME", "My Series", 2014)]
        [TestCase("Series.Saison3.VOSTFR.HDTV.XviD-NOTAG", "Series", 3)]
        [TestCase("Series.SAISON.1.VFQ.PDTV.H264-ACC-ROLLED", "Series", 1)]
        [TestCase("Series Title - Series 1 (1970) DivX", "Series Title", 1)]
        [TestCase("SeriesTitle.S03.540p.AMZN.WEB-DL.DD+2.0.x264-RTN", "SeriesTitle", 3)]
        public void should_parse_full_season_release(string postTitle, string title, int season)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(season);
            result.SeriesTitle.Should().Be(title);
            result.EpisodeNumbers.Should().BeEmpty();
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeTrue();
        }

        [TestCase("Acropolis Series S05 EXTRAS DVDRip XviD RUNNER", "Acropolis Series", 5)]
        [TestCase("Punky Series S01 EXTRAS DVDRip XviD RUNNER", "Punky Series", 1)]
        [TestCase("Instant Series S03 EXTRAS DVDRip XviD OSiTV", "Instant Series", 3)]
        [TestCase("The.Series.S03.Extras.01.Deleted.Scenes.720p", "The Series", 3)]
        [TestCase("The.Series.S03.Extras.02.720p", "The Series", 3)]
        public void should_parse_season_extras(string postTitle, string title, int season)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(season);
            result.SeriesTitle.Should().Be(title);
            result.EpisodeNumbers.Should().BeEmpty();
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeTrue();
            result.IsSeasonExtra.Should().BeTrue();
        }

        [TestCase("Series.to.Me.S03.SUBPACK.DVDRip.XviD-REWARD", "Series to Me", 3)]
        [TestCase("The.Series.S02.SUBPACK.DVDRip.XviD-REWARD", "The Series", 2)]
        [TestCase("Series.S11.SUBPACK.DVDRip.XviD-REWARD", "Series", 11)]
        public void should_parse_season_subpack(string postTitle, string title, int season)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(season);
            result.SeriesTitle.Should().Be(title);
            result.EpisodeNumbers.Should().BeEmpty();
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeTrue();
            result.IsSeasonExtra.Should().BeTrue();
        }

        [TestCase("The.Series.2016.S02.Part.1.1080p.NF.WEBRip.DD5.1.x264-NTb", "The Series 2016", 2, 1)]
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

        [TestCase("The Series S01-05 WS BDRip X264-REWARD-No Rars", "The Series", 1)]
        [TestCase("Series.Title.S01-S09.1080p.AMZN.WEB-DL.DDP2.0.H.264-NTb", "Series Title", 1)]
        [TestCase("Series Title S01 - S07 BluRay 1080p x264 REPACK -SacReD", "Series Title", 1)]
        [TestCase("Series Title Season 01-07 BluRay 1080p x264 REPACK -SacReD", "Series Title", 1)]
        [TestCase("Series Title Season 01 - Season 07 BluRay 1080p x264 REPACK -SacReD", "Series Title", 1)]
        public void should_parse_multi_season_release(string postTitle, string title, int firstSeason)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(firstSeason);
            result.SeriesTitle.Should().Be(title);
            result.EpisodeNumbers.Should().BeEmpty();
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeTrue();
            result.IsPartialSeason.Should().BeFalse();
            result.IsMultiSeason.Should().BeTrue();
        }

        [Test]
        public void should_not_parse_season_folders()
        {
            var result = Parser.Parser.ParseTitle("Season 3");
            result.Should().BeNull();
        }
    }
}
