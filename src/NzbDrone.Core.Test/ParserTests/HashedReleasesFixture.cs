using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class hashedReleasesFixture : CoreTest
    {
        [TestCase(@"C:\Test\Some.Hashed.Release.S01E01.720p.WEB-DL.AAC2.0.H.264-Mercury\0e895c3724.mkv", "somehashedrelease", "WEBDL-720p", "Mercury")]
        [TestCase(@"C:\Test\0e895c3724\Some.Hashed.Release.S01E01.720p.WEB-DL.AAC2.0.H.264-Mercury.mkv", "somehashedrelease", "WEBDL-720p", "Mercury")]
        public void should_properly_parse_hashed_releases(string path, string title, string quality, string releaseGroup)
        {
            var result = Parser.Parser.ParsePath(path);
            result.SeriesTitle.Should().Be(title);
            result.Quality.ToString().Should().Be(quality);
            result.ReleaseGroup.Should().Be(releaseGroup);
        }
    }
}
