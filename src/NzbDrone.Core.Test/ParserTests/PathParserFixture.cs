using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class PathParserFixture : CoreTest
    {
        [TestCase(@"z:\tv shows\series title (2003)\Season 3\S03E05 - Title.mkv", 3, 5)]
        [TestCase(@"z:\tv showsseries title\Season 16\S16E03 - The Title.mkv", 16, 3)]
        [TestCase(@"z:\tv shows\series title\Specials\S00E16 - Dear Title - SD TV.avi", 0, 16)]
        [TestCase(@"D:\shares\TV Shows\series title\Season 2\S02E21 - 94 Title - 720p TV.mkv", 2, 21)]
        [TestCase(@"D:\shares\TV Shows\Series (2003)\Season 2\S02E21.avi", 2, 21)]
        [TestCase("C:/Test/TV/Series.4x05.HDTV.XviD-LOL", 4, 5)]
        [TestCase(@"P:\TV Shows\Series\Season 6\S06E13 - 5 to 9 - 720p BluRay.mkv", 6, 13)]
        [TestCase(@"S:\TV Drop\Series - 10x11 - Title [SDTV]\1011 - Title.avi", 10, 11)]
        [TestCase(@"/TV Drop/Series - 10x11 - Title [SDTV]/1011 - Title.avi", 10, 11)]
        [TestCase(@"S:\TV Drop\Series Title - 10x12 - 24 Hours of Development [SDTV]\1012 - Hours of Development.avi", 10, 12)]
        [TestCase(@"/TV Drop/Series Title - 10x12 - 24 Hours of Development [SDTV]/1012 - Hours of Development.avi", 10, 12)]
        [TestCase(@"S:\TV Drop\Series Title - 10x12 - 24 Hours of Development [SDTV]\Hours of Development.avi", 10, 12)]
        [TestCase(@"/TV Drop/Series Title - 10x12 - 24 Hours of Development [SDTV]/Hours of Development.avi", 10, 12)]
        [TestCase(@"E:\Downloads\tv\Series.Title.S01E01.720p.HDTV\ajifajjjeaeaeqwer_eppj.avi", 1, 1)]
        [TestCase(@"C:\Test\Unsorted\Series.Title.S01E01.720p.HDTV\tbbt101.avi", 1, 1)]
        [TestCase(@"C:\Test\Unsorted\Series.Title.S02E19.720p.BluRay.x264-SiNNERS-RP\ba27283b17c00d01193eacc02a8ba98eeb523a76.mkv", 2, 19)]
        [TestCase(@"C:\Test\Unsorted\Series.Title.S02E18.720p.BluRay.x264-SiNNERS-RP\45a55debe3856da318cc35882ad07e43cd32fd15.mkv", 2, 18)]
        [TestCase(@"C:\Test\Series\Season 01\01 Pilot (1080p HD).mkv", 1, 1)]
        [TestCase(@"C:\Test\Series\Season 01\1 Pilot (1080p HD).mkv", 1, 1)]
        [TestCase(@"C:\Test\Series\Season 1\02 Honor Thy Father (1080p HD).m4v", 1, 2)]
        [TestCase(@"C:\Test\Series\Season 1\2 Honor Thy Developer (1080p HD).m4v", 1, 2)]

        //[TestCase(@"C:\series.state.S02E04.720p.WEB-DL.DD5.1.H.264\73696S02-04.mkv", 2, 4)] //Gets treated as S01E04 (because it gets parsed as anime); 2020-01 broken test case: Expected result.EpisodeNumbers to contain 1 item(s), but found 0
        public void should_parse_from_path(string path, int season, int episode)
        {
            var result = Parser.Parser.ParsePath(path.AsOsAgnostic());

            result.EpisodeNumbers.Should().HaveCount(1);
            result.SeasonNumber.Should().Be(season);
            result.EpisodeNumbers[0].Should().Be(episode);
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("01-03\\The Series Title (2010) - 1x01-02-03 - Episode Title HDTV-720p Proper", "The Series Title (2010)", 1, new[] { 1, 2, 3 })]
        public void should_parse_multi_episode_from_path(string path, string title, int season, int[] episodes)
        {
            var result = Parser.Parser.ParsePath(path.AsOsAgnostic());

            result.SeriesTitle.Should().Be(title);
            result.EpisodeNumbers.Should().HaveCount(episodes.Length);
            result.SeasonNumber.Should().Be(season);
            result.EpisodeNumbers.Should().BeEquivalentTo(episodes);
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();

            ExceptionVerification.IgnoreWarns();
        }
    }
}
