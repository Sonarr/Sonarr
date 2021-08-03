using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class AnimeMetadataParserFixture : CoreTest
    {
        [TestCase("[SubDESU]_Show_Title_DxD_07_(1280x720_x264-AAC)_[6B7FD717]", "SubDESU", "6B7FD717")]
        [TestCase("[Chihiro]_Show_Title!!_-_06_[848x480_H.264_AAC][859EEAFA]", "Chihiro", "859EEAFA")]
        [TestCase("[Underwater]_Show_Title_-_12_(720p)_[5C7BC4F9]", "Underwater", "5C7BC4F9")]
        [TestCase("[HorribleSubs]_Show_Title_-_33_[720p]", "HorribleSubs", "")]
        [TestCase("[HorribleSubs] Show-Title - 13 [1080p].mkv", "HorribleSubs", "")]
        [TestCase("[Doremi].Show.Title.5.Go.Go!.31.[1280x720].[C65D4B1F].mkv", "Doremi", "C65D4B1F")]
        [TestCase("[Doremi].Show.Title.5.Go.Go!.31[1280x720].[C65D4B1F]", "Doremi", "C65D4B1F")]
        [TestCase("[Doremi].Show.Title.5.Go.Go!.31.[1280x720].mkv", "Doremi", "")]
        [TestCase("[K-F] Series Title 214", "K-F", "")]
        [TestCase("[K-F] Series Title S10E14 214", "K-F", "")]
        [TestCase("[K-F] Series Title 10x14 214", "K-F", "")]
        [TestCase("[K-F] Series Title 214 10x14", "K-F", "")]
        [TestCase("Series Title - 031 - The Resolution to Kill [Lunar].avi", "Lunar", "")]
        [TestCase("[ACX]Series Title 01 Episode Name [Kosaka] [9C57891E].mkv", "ACX", "9C57891E")]
        [TestCase("[S-T-D] Series Title! - 06 (1280x720 10bit AAC) [59B3F2EA].mkv", "S-T-D", "59B3F2EA")]
        public void should_parse_absolute_numbers(string postTitle, string subGroup, string hash)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.ReleaseGroup.Should().Be(subGroup);
            result.ReleaseHash.Should().Be(hash);
        }
    }
}
