using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class HashedReleaseFixture : CoreTest
    {
        public static object[] HashedReleaseParserCases =
        {
            new object[]
            {
                @"C:\Test\Some.Hashed.Release.S01E01.720p.WEB-DL.AAC2.0.H.264-Mercury\0e895c3724.mkv".AsOsAgnostic(),
                "somehashedrelease",
                "WEBDL-720p",
                "Mercury"
            },
            new object[]
            {
                @"C:\Test\0e895c3724\Some.Hashed.Release.S01E01.720p.WEB-DL.AAC2.0.H.264-Mercury.mkv".AsOsAgnostic(),
                "somehashedrelease",
                "WEBDL-720p",
                "Mercury"
            }
        };

        [Test, TestCaseSource("HashedReleaseParserCases")]
        public void should_properly_parse_hashed_releases(string path, string title, string quality, string releaseGroup)
        {
            var result = Parser.Parser.ParsePath(path);
            result.SeriesTitle.Should().Be(title);
            result.Quality.ToString().Should().Be(quality);
            result.ReleaseGroup.Should().Be(releaseGroup);
        }
    }
}
