using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class ParserFixture : CoreTest
    {
        /*Fucked-up hall of shame,
         * WWE.Wrestlemania.27.PPV.HDTV.XviD-KYR
         * Unreported.World.Chinas.Lost.Sons.WS.PDTV.XviD-FTP
         * [TestCase("Big Time Rush 1x01 to 10 480i DD2 0 Sianto", "Big Time Rush", 1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10)]
         * [TestCase("Desparate Housewives - S07E22 - 7x23 - And Lots of Security.. [HDTV-720p].mkv", "Desparate Housewives", 7, new[] { 22, 23 }, 2)]
         * [TestCase("S07E22 - 7x23 - And Lots of Security.. [HDTV-720p].mkv", "", 7, new[] { 22, 23 }, 2)]
         * (Game of Thrones s03 e - "Game of Thrones Season 3 Episode 10"
         * The.Man.of.Steel.1994-05.33.hybrid.DreamGirl-Novus-HD
         * Superman.-.The.Man.of.Steel.1994-06.34.hybrid.DreamGirl-Novus-HD
         * Superman.-.The.Man.of.Steel.1994-05.33.hybrid.DreamGirl-Novus-HD
         */

        [TestCase("[SubDESU]_High_School_DxD_07_(1280x720_x264-AAC)_[6B7FD717]", "High School DxD", 7, 0, 0)]
        [TestCase("[Chihiro]_Working!!_-_06_[848x480_H.264_AAC][859EEAFA]", "Working!!", 6, 0, 0)]
        [TestCase("[Commie]_Senki_Zesshou_Symphogear_-_11_[65F220B4]", "Senki_Zesshou_Symphogear", 11, 0, 0)]
        [TestCase("[Underwater]_Rinne_no_Lagrange_-_12_(720p)_[5C7BC4F9]", "Rinne_no_Lagrange", 12, 0, 0)]
        [TestCase("[Commie]_Rinne_no_Lagrange_-_15_[E76552EA]", "Rinne_no_Lagrange", 15, 0, 0)]
        [TestCase("[HorribleSubs]_Hunter_X_Hunter_-_33_[720p]", "Hunter_X_Hunter", 33, 0, 0)]
        [TestCase("[HorribleSubs]_Fairy_Tail_-_145_[720p]", "Fairy_Tail", 145, 0, 0)]
        [TestCase("[HorribleSubs] Tonari no Kaibutsu-kun - 13 [1080p].mkv", "Tonari no Kaibutsu-kun", 13, 0, 0)]
        [TestCase("[Doremi].Yes.Pretty.Cure.5.Go.Go!.31.[1280x720].[C65D4B1F].mkv", "Yes.Pretty.Cure.5.Go.Go!", 31, 0, 0)]
        [TestCase("[K-F] One Piece 214", "One Piece", 214, 0, 0)]
        [TestCase("[K-F] One Piece S10E14 214", "One Piece", 214, 10, 14)]
        [TestCase("[K-F] One Piece 10x14 214", "One Piece", 214, 10, 14)]
        [TestCase("[K-F] One Piece 214 10x14", "One Piece", 214, 10, 14)]
//        [TestCase("One Piece S10E14 214", "One Piece", 214, 10, 14)]
//        [TestCase("One Piece 10x14 214", "One Piece", 214, 10, 14)]
//        [TestCase("One Piece 214 10x14", "One Piece", 214, 10, 14)]
//        [TestCase("214 One Piece 10x14", "One Piece", 214, 10, 14)]
        [TestCase("Bleach - 031 - The Resolution to Kill [Lunar].avi", "Bleach", 31, 0, 0)]
        [TestCase("Bleach - 031 - The Resolution to Kill [Lunar]", "Bleach", 31, 0, 0)]
        [TestCase("[ACX]Hack Sign 01 Role Play [Kosaka] [9C57891E].mkv", "Hack Sign", 1, 0, 0)]
        [TestCase("[SFW-sage] Bakuman S3 - 12 [720p][D07C91FC]", "Bakuman S3", 12, 0, 0)]
        [TestCase("ducktales_e66_time_is_money_part_one_marking_time", "DuckTales", 66, 0, 0)]
        public void parse_absolute_numbers(string postTitle, string title, int absoluteEpisodeNumber, int seasonNumber, int episodeNumber)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.AbsoluteEpisodeNumbers.First().Should().Be(absoluteEpisodeNumber);
            result.SeasonNumber.Should().Be(seasonNumber);
            result.EpisodeNumbers.FirstOrDefault().Should().Be(episodeNumber);
            result.SeriesTitle.Should().Be(title.CleanSeriesTitle());
            result.FullSeason.Should().BeFalse();
        }

        [TestCase("Chuck - 4x05 - Title", "Chuck")]
        [TestCase("Law & Order - 4x05 - Title", "laworder")]
        [TestCase("Bad Format", "badformat")]
        [TestCase("Mad Men - Season 1 [Bluray720p]", "madmen")]
        [TestCase("Mad Men - Season 1 [Bluray1080p]", "madmen")]
        [TestCase("The Daily Show With Jon Stewart -", "dailyshowwithjonstewart")]
        [TestCase("The Venture Bros. (2004)", "venturebros2004")]
        [TestCase("Castle (2011)", "castle2011")]
        [TestCase("Adventure Time S02 720p HDTV x264 CRON", "adventuretime")]
        [TestCase("Hawaii Five 0", "hawaiifive0")]
        [TestCase("Match of the Day", "matchday")]
        [TestCase("Match of the Day 2", "matchday2")]
        public void should_parse_series_name(string postTitle, string title)
        {
            var result = Parser.Parser.ParseSeriesName(postTitle);
            result.Should().Be(title.CleanSeriesTitle());
        }
    }
}
