using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class AnimeVersionFixture : CoreTest
    {
        [TestCase("Anime Title - S01E01 - (BD 1080p HEVC FLAC) [Dual Audio] [Group]", 1)]
        [TestCase("Anime Title - S01E01v2 - (BD 1080p HEVC FLAC) [Dual Audio] [Group]", 2)]
        [TestCase("Anime Title - S01E01 v2 - (BD 1080p HEVC FLAC) [Dual Audio] [Group]", 2)]
        [TestCase("[SubsPlease] Anime Title - 01 (1080p) [B1F227CF]", 1)]
        [TestCase("[SubsPlease] Anime Title - 01v2 (1080p) [B1F227CF]", 2)]
        [TestCase("[SubsPlease] Anime Title - 01 v2 (1080p) [B1F227CF]", 2)]
        public void should_be_able_to_parse_repack(string title, int version)
        {
            var result = QualityParser.ParseQuality(title);
            result.Revision.Version.Should().Be(version);
        }
    }
}
