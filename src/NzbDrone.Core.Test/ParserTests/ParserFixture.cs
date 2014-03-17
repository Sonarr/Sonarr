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

        [TestCase("Chuck - 4x05 - Title", "Chuck")]
        [TestCase("Law & Order - 4x05 - Title", "laworder")]
        [TestCase("Bad Format", "badformat")]
        [TestCase("Mad Men - Season 1 [Bluray720p]", "madmen")]
        [TestCase("Mad Men - Season 1 [Bluray1080p]", "madmen")]
        [TestCase("The Daily Show With Jon Stewart -", "thedailyshowwithjonstewart")]
        [TestCase("The Venture Bros. (2004)", "theventurebros2004")]
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
