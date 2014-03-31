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
    public class PathParserFixture : CoreTest
    {
        [TestCase(@"z:\tv shows\battlestar galactica (2003)\Season 3\S03E05 - Collaborators.mkv", 3, 5)]
        [TestCase(@"z:\tv shows\modern marvels\Season 16\S16E03 - The Potato.mkv", 16, 3)]
        [TestCase(@"z:\tv shows\robot chicken\Specials\S00E16 - Dear Consumer - SD TV.avi", 0, 16)]
        [TestCase(@"D:\shares\TV Shows\Parks And Recreation\Season 2\S02E21 - 94 Meetings - 720p TV.mkv", 2, 21)]
        [TestCase(@"D:\shares\TV Shows\Battlestar Galactica (2003)\Season 2\S02E21.avi", 2, 21)]
        [TestCase("C:/Test/TV/Chuck.4x05.HDTV.XviD-LOL", 4, 5)]
        [TestCase(@"P:\TV Shows\House\Season 6\S06E13 - 5 to 9 - 720p BluRay.mkv", 6, 13)]
        [TestCase(@"S:\TV Drop\House - 10x11 - Title [SDTV]\1011 - Title.avi", 10, 11)]
        [TestCase(@"/TV Drop/House - 10x11 - Title [SDTV]/1011 - Title.avi", 10, 11)]
        [TestCase(@"S:\TV Drop\King of the Hill - 10x12 - 24 Hour Propane People [SDTV]\1012 - 24 Hour Propane People.avi", 10, 12)]
        [TestCase(@"/TV Drop/King of the Hill - 10x12 - 24 Hour Propane People [SDTV]/1012 - 24 Hour Propane People.avi", 10, 12)]
        [TestCase(@"S:\TV Drop\King of the Hill - 10x12 - 24 Hour Propane People [SDTV]\Hour Propane People.avi", 10, 12)]
        [TestCase(@"/TV Drop/King of the Hill - 10x12 - 24 Hour Propane People [SDTV]/Hour Propane People.avi", 10, 12)]
        [TestCase(@"E:\Downloads\tv\The.Big.Bang.Theory.S01E01.720p.HDTV\ajifajjjeaeaeqwer_eppj.avi", 1, 1)]
        [TestCase(@"C:\Test\Unsorted\The.Big.Bang.Theory.S01E01.720p.HDTV\tbbt101.avi", 1, 1)]
        [TestCase(@"C:\Test\Unsorted\Terminator.The.Sarah.Connor.Chronicles.S02E19.720p.BluRay.x264-SiNNERS-RP\ba27283b17c00d01193eacc02a8ba98eeb523a76.mkv", 2, 19)]
        [TestCase(@"C:\Test\Unsorted\Terminator.The.Sarah.Connor.Chronicles.S02E18.720p.BluRay.x264-SiNNERS-RP\45a55debe3856da318cc35882ad07e43cd32fd15.mkv", 2, 18)]
        [TestCase(@"C:\Test\The.Blacklist.S01E16.720p.HDTV.X264-DIMENSION\XRmZciqkBopq4851Ddbipe\Vh1FvU3bJXw6zs8EEUX4bMo5vbbMdHghxHirc.mkv", 1, 16)]
        public void should_parse_from_path(string path, int season, int episode)
        {
            var result = Parser.Parser.ParsePath(path);
            result.EpisodeNumbers.Should().HaveCount(1);
            result.SeasonNumber.Should().Be(season);
            result.EpisodeNumbers[0].Should().Be(episode);
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();

            ExceptionVerification.IgnoreWarns();
        }
    }
}
