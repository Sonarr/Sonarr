using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Expansive;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class MultiEpisodeParserFixture : CoreTest
    {
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", "WEEDS", 3, new[] { 1, 2, 3, 4, 5, 6 })]
        [TestCase("Two.and.a.Half.Men.103.104.720p.HDTV.X264-DIMENSION", "Two.and.a.Half.Men", 1, new[] { 3, 4 })]
        [TestCase("Weeds.S03E01.S03E02.720p.HDTV.X264-DIMENSION", "Weeds", 3, new[] { 1, 2 })]
        [TestCase("The Borgias S01e01 e02 ShoHD On Demand 1080i DD5 1 ALANiS", "The Borgias", 1, new[] { 1, 2 })]
        [TestCase("White.Collar.2x04.2x05.720p.BluRay-FUTV", "White.Collar", 2, new[] { 4, 5 })]
        [TestCase("Desperate.Housewives.S07E22E23.720p.HDTV.X264-DIMENSION", "Desperate.Housewives", 7, new[] { 22, 23 })]
        [TestCase("Desparate Housewives - S07E22 - S07E23 - And Lots of Security.. [HDTV-720p].mkv", "Desparate Housewives", 7, new[] { 22, 23 })]
        [TestCase("S03E01.S03E02.720p.HDTV.X264-DIMENSION", "", 3, new[] { 1, 2 })]
        [TestCase("Desparate Housewives - S07E22 - 7x23 - And Lots of Security.. [HDTV-720p].mkv", "Desparate Housewives", 7, new[] { 22, 23 })]
        [TestCase("S07E22 - 7x23 - And Lots of Security.. [HDTV-720p].mkv", "", 7, new[] { 22, 23 })]
        [TestCase("2x04x05.720p.BluRay-FUTV", "", 2, new[] { 4, 5 })]
        [TestCase("S02E04E05.720p.BluRay-FUTV", "", 2, new[] { 4, 5 })]
        [TestCase("S02E03-04-05.720p.BluRay-FUTV", "", 2, new[] { 3, 4, 5 })]
        [TestCase("Breakout.Kings.S02E09-E10.HDTV.x264-ASAP", "Breakout Kings", 2, new[] { 9, 10 })]
        [TestCase("Breakout Kings - 2x9-2x10 - Served Cold [SDTV] ", "Breakout Kings", 2, new[] { 9, 10 })]
        [TestCase("Breakout Kings - 2x09-2x10 - Served Cold [SDTV] ", "Breakout Kings", 2, new[] { 9, 10 })]
        [TestCase("Hell on Wheels S02E09 E10 HDTV x264 EVOLVE", "Hell on Wheels", 2, new[] { 9, 10 })]
        [TestCase("Hell.on.Wheels.S02E09-E10.720p.HDTV.x264-EVOLVE", "Hell on Wheels", 2, new[] { 9, 10 })]
        [TestCase("Grey's Anatomy - 8x01_02 - Free Falling", "Grey's Anatomy", 8, new [] { 1,2 })]
        [TestCase("8x01_02 - Free Falling", "", 8, new[] { 1, 2 })]
        [TestCase("Kaamelott.S01E91-E100", "Kaamelott", 1, new[] { 91, 92, 93, 94, 95, 96, 97, 98, 99, 100 })]
        [TestCase("Neighbours.S29E161-E165.PDTV.x264-FQM", "Neighbours", 29, new[] { 161, 162, 163, 164, 165 })]
        [TestCase("Shortland.Street.S22E5363-E5366.HDTV.x264-FiHTV", "Shortland Street", 22, new[] { 5363, 5364, 5365, 5366 })]
        public void should_parse_multiple_episodes(string postTitle, string title, int season, int[] episodes)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(season);
            result.EpisodeNumbers.Should().BeEquivalentTo(episodes);
            result.SeriesTitle.Should().Be(title.CleanSeriesTitle());
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();
        }
    }
}
