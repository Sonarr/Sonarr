using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class MultiEpisodeParserFixture : CoreTest
    {
        [TestCase("Series.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", "Series", 3, new[] { 1, 2, 3, 4, 5, 6 })]
        [TestCase("Series.Title.103.104.720p.HDTV.X264-DIMENSION", "Series Title", 1, new[] { 3, 4 })]
        [TestCase("Series.S03E01.S03E02.720p.HDTV.X264-DIMENSION", "Series", 3, new[] { 1, 2 })]
        [TestCase("The Series S01e01 e02 ShoHD On Demand 1080i DD5 1 ALANiS", "The Series", 1, new[] { 1, 2 })]
        [TestCase("Series.Title.2x04.2x05.720p.BluRay-FUTV", "Series Title", 2, new[] { 4, 5 })]
        [TestCase("Series.Title.S07E22E23.720p.HDTV.X264-DIMENSION", "Series Title", 7, new[] { 22, 23 })]
        [TestCase("Series Title - S07E22 - S07E23 - And Lots of Security.. [HDTV-720p].mkv", "Series Title", 7, new[] { 22, 23 })]
        [TestCase("S03E01.S03E02.720p.HDTV.X264-DIMENSION", "", 3, new[] { 1, 2 })]
        [TestCase("2x04x05.720p.BluRay-FUTV", "", 2, new[] { 4, 5 })]
        [TestCase("S02E04E05.720p.BluRay-FUTV", "", 2, new[] { 4, 5 })]
        [TestCase("S02E03-04-05.720p.BluRay-FUTV", "", 2, new[] { 3, 4, 5 })]
        [TestCase("Series.Kings.S02E09-E10.HDTV.x264-ASAP", "Series Kings", 2, new[] { 9, 10 })]
        [TestCase("Series Kings - 2x9-2x10 - Served Code [SDTV] ", "Series Kings", 2, new[] { 9, 10 })]
        [TestCase("Series Kings - 2x09-2x10 - Served Code [SDTV] ", "Series Kings", 2, new[] { 9, 10 })]
        [TestCase("Hell on Series S02E09 E10 HDTV x264 EVOLVE", "Hell on Series", 2, new[] { 9, 10 })]
        [TestCase("Hell.on.Series.S02E09-E10.720p.HDTV.x264-EVOLVE", "Hell on Series", 2, new[] { 9, 10 })]
        [TestCase("Series's Sonarr - 8x01_02 - Free Falling", "Series's Sonarr", 8, new[] { 1, 2 })]
        [TestCase("8x01_02 - Free Falling", "", 8, new[] { 1, 2 })]
        [TestCase("Series.S01E91-E100", "Series", 1, new[] { 91, 92, 93, 94, 95, 96, 97, 98, 99, 100 })]
        [TestCase("Series.S29E161-E165.PDTV.x264-FQM", "Series", 29, new[] { 161, 162, 163, 164, 165 })]
        [TestCase("Shortland.Series.S22E5363-E5366.HDTV.x264-FiHTV", "Shortland Series", 22, new[] { 5363, 5364, 5365, 5366 })]
        [TestCase("the.Series.101.102.hdtv-lol", "the Series", 1, new[] { 1, 2 })]
        [TestCase("Series.10708.hdtv-lol.mp4", "Series", 1, new[] { 7, 8 })]
        [TestCase("Series.10910.hdtv-lol.mp4", "Series", 1, new[] { 9, 10 })]
        [TestCase("E.010910.HDTVx264REPACKLOL.mp4", "E", 1, new[] { 9, 10 })]
        [TestCase("World Series of Sonarr - 2010x15 - 2010x16 - HD TV.mkv", "World Series of Sonarr", 2010, new[] { 15, 16 })]
        [TestCase("The Series US S01E01-E02 720p HDTV x264", "The Series US", 1, new[] { 1, 2 })]
        [TestCase("Series Title Season 01 Episode 05-06 720p", "Series Title", 1, new[] { 5, 6 })]

        //[TestCase("My Name Is Sonarr - S03E01-E02 - My Name Is Code 28301-016 [SDTV]", "My Name Is Sonarr", 3, new[] { 1, 2 })]
        //[TestCase("Adventure Series - 5x01 - x02 - Dev the Human (2) & Sonarr the Robot (3)", "Adventure Series", 5, new [] { 1, 2 })]
        [TestCase("The Series And The Code - S42 Ep10718 - Ep10722", "The Series And The Code", 42, new[] { 10718, 10719, 10720, 10721, 10722 })]
        [TestCase("The Series And The Code - S42 Ep10688 - Ep10692", "The Series And The Code", 42, new[] { 10688, 10689, 10690, 10691, 10692 })]
        [TestCase("Series.S01E02E03.1080p.BluRay.x264-DeBTViD", "Series", 1, new[] { 2, 3 })]
        [TestCase("grp-zoos01e11e12-1080p", "grp-zoo", 1, new[] { 11, 12 })]
        [TestCase("grp-zoo-s01e11e12-1080p", "grp-zoo", 1, new[] { 11, 12 })]
        [TestCase("Series Title.S6.E1.E2.Episode Name.1080p.WEB-DL", "Series Title", 6, new[] { 1, 2 })]
        [TestCase("Series Title.S6E1-E2.Episode Name.1080p.WEB-DL", "Series Title", 6, new[] { 1, 2 })]
        [TestCase("Series Title.S6E1-S6E2.Episode Name.1080p.WEB-DL", "Series Title", 6, new[] { 1, 2 })]
        [TestCase("Series Title.S6E1E2.Episode Name.1080p.WEB-DL", "Series Title", 6, new[] { 1, 2 })]
        [TestCase("Series Title.S6E1-E2-E3.Episode Name.1080p.WEB-DL", "Series Title", 6, new[] { 1, 2, 3 })]
        [TestCase("Series Title.S6.E1E3.Episode Name.1080p.WEB-DL", "Series Title", 6, new[] { 1, 2, 3 })]
        [TestCase("Series Title.S6.E1-E2.Episode Name.1080p.WEB-DL", "Series Title", 6, new[] { 1, 2 })]
        [TestCase("Series Title.S6.E1-S6E2.Episode Name.1080p.WEB-DL", "Series Title", 6, new[] { 1, 2 })]
        [TestCase("Series Title.S6.E1E2.Episode Name.1080p.WEB-DL", "Series Title", 6, new[] { 1, 2 })]
        [TestCase("Series Title.S6.E1-E2-E3.Episode Name.1080p.WEB-DL", "Series Title", 6, new[] { 1, 2, 3 })]
        [TestCase("Series.Title.S05E01-E02.720p.5.1Ch.BluRay", "Series Title", 5, new[] { 1, 2 })]
        [TestCase("Series.Title.S05E01-02.720p.5.1Ch.BluRay", "Series Title", 5, new[] { 1, 2 })]
        [TestCase("S01E01-E03 - Episode Title.HDTV-720p", "", 1, new[] { 1, 2, 3 })]
        [TestCase("1x01-x03 - Episode Title.HDTV-720p", "", 1, new[] { 1, 2, 3 })]
        [TestCase("Series.Title.E07-E08.180612.1080p-NEXT", "Series Title", 1, new[] { 7, 8 })]
        [TestCase("Series Title? E11-E12 1080p HDTV AAC H.264-NEXT", "Series Title", 1, new[] { 11, 12 })]
        [TestCase("The Series Title (2010) - [S01E01-02-03] - Episode Title", "The Series Title (2010)", 1, new[] { 1, 2, 3 })]
        [TestCase("[AqusiQ-TorrentS.pl]The.Name.of.the.Series.S01E05-06.PL.2160p-Ralf[shogho]", "The Name of the Series", 1, new[] { 5, 6 })]
        [TestCase("[AgusiQ-TorrentS.pl] The.Name.of.the.Series.S01E05-E06.PL.1080i.Ralf [jans12]", "The Name of the Series", 1, new[] { 5, 6 })]
        [TestCase("The.Name.of.the.Series.S01E05-6.PL.1080p.WEBRip.x264-666", "The Name of the Series", 1, new[] { 5, 6 })]
        [TestCase("Series Title - S15E06-07 - City Sushi HDTV-720p", "Series Title", 15, new[] { 6, 7 })]
        [TestCase("Series Title - S01E01-02-03 - Episode Title HDTV-720p", "Series Title", 1, new[] { 1, 2, 3 })]
        [TestCase("Series Title - [02x01x02] - Episode 1", "Series Title", 2, new[] { 1, 2 })]
        [TestCase("Series Title - [02x01-x02] - Episode 1", "Series Title", 2, new[] { 1, 2 })]
        [TestCase("Series Title - [02x01-02] - Episode 1", "Series Title", 2, new[] { 1, 2 })]
        [TestCase("Series Title (2011) - S01E23-E24 - ...E i nuovi orizzonti [HDTV 360p] [ITA].mkv", "Series Title (2011)", 1, new[] { 23, 24 })]
        [TestCase("The Series Title! - S01E01-02-03", "The Series Title!", 1, new[] { 1, 2, 3 })]
        [TestCase("Series Title! (2013) - S04E44-E45 - Il 200 spettacolare episodio da narcisisti!", "Series Title! (2013)", 4, new[] { 44, 45 })]
        [TestCase("Series Title! (2013) - S04E44-E45 - Il 200 spettacolare episodio da narcisisti! [NetflixHD 720p HEVC] [ITA+ENG].mkv", "Series Title! (2013)", 4, new[] { 44, 45 })]

        //[TestCase("", "", , new [] {  })]
        public void should_parse_multiple_episodes(string postTitle, string title, int season, int[] episodes)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(season);
            result.EpisodeNumbers.Should().BeEquivalentTo(episodes);
            result.SeriesTitle.Should().Be(title);
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();
        }
    }
}
