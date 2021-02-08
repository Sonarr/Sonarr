using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ExtendedQualityParserRegex : CoreTest
    {
        [TestCase("Series.Title.S04E05.HDTV.XviD-LOL", 0)]
        [TestCase("Series.Title.S04E05.Garnets.or.Gold.REAL.REAL.PROPER.HDTV.x264-W4F", 2)]
        [TestCase("Series.Title.S03E17.REAL.PROPER.720p.HDTV.x264-ORENJI-RP", 1)]
        [TestCase("Series.Title.S05E09.REAL.PROPER.HDTV.x264-KILLERS", 1)]
        [TestCase("Series.Title.S14E01.REAL.PROPER.720p.HDTV.x264-KILLERS", 1)]
        [TestCase("Series.Title.s02e06.real.proper.720p.webrip.x264-2hd", 0)]
        [TestCase("Series.Title.S21E07.Super.Duper.Real.Proper.HDTV.x264-FTP", 0)]
        [TestCase("Series.Title.S21E07.PROPER.HDTV.x264-RiVER-RP", 0)]
        [TestCase("Series.Title.S07E11.PROPER.REAL.RERIP.1080p.BluRay.x264-TENEIGHTY", 1)]
        [TestCase("[MGS] - Series Title - Episode 02v2 - [D8B6C90D]", 0)]
        [TestCase("[Hatsuyuki] Series Title - 07 [v2][848x480][23D8F455].avi", 0)]
        [TestCase("[DeadFish] Series Title - 01v3 [720p][AAC]", 0)]
        [TestCase("[DeadFish] Series Title - 01v4 [720p][AAC]", 0)]
        [TestCase("The Real Housewives of Some Place - S01E01 - Why are we doing this?", 0)]
        public void should_parse_reality_from_title(string title, int reality)
        {
            QualityParser.ParseQuality(title).Revision.Real.Should().Be(reality);
        }

        [TestCase("Series.Title.S04E05.HDTV.XviD-LOL", 1)]
        [TestCase("Series.Title.S04E05.Garnets.or.Gold.REAL.REAL.PROPER.HDTV.x264-W4F", 2)]
        [TestCase("Series.Title.S03E17.REAL.PROPER.720p.HDTV.x264-ORENJI-RP", 2)]
        [TestCase("Series.Title.S05E09.REAL.PROPER.HDTV.x264-KILLERS", 2)]
        [TestCase("Series.Title.S14E01.REAL.PROPER.720p.HDTV.x264-KILLERS", 2)]
        [TestCase("Series.Title.Is.the.New.Black.s02e06.real.proper.720p.webrip.x264-2hd", 2)]
        [TestCase("Series.Title.S21E07.Super.Duper.Real.Proper.HDTV.x264-FTP", 2)]
        [TestCase("Series.Title.S21E07.PROPER.HDTV.x264-RiVER-RP", 2)]
        [TestCase("Series.Title.S07E11.PROPER.REAL.RERIP.1080p.BluRay.x264-TENEIGHTY", 2)]
        [TestCase("[MGS] - Series Title - Episode 02v2 - [D8B6C90D]", 2)]
        [TestCase("[Hatsuyuki] Series Title - 07 [v2][848x480][23D8F455].avi", 2)]
        [TestCase("[DeadFish] Series Title - 01v3 [720p][AAC]", 3)]
        [TestCase("[DeadFish]Series Title - 01v4 [720p][AAC]", 4)]
        [TestCase("[Vivid-Asenshi] Series Title - 04v2 [266EE983]", 2)]
        [TestCase("[Vivid-Asenshi] Series Title - 03v2 [66A05817]", 2)]
        [TestCase("[Vivid-Asenshi] Series Title - 02v2 [1F67AB55]", 2)]
        public void should_parse_version_from_title(string title, int version)
        {
            QualityParser.ParseQuality(title).Revision.Version.Should().Be(version);
        }
    }
}
