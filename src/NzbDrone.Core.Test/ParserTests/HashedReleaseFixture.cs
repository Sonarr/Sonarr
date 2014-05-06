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
                @"C:\Test\Some.Hashed.Release.S01E01.720p.WEB-DL.AAC2.0.H.264-Mercury\0e895c37245186812cb08aab1529cf8ee389dd05.mkv".AsOsAgnostic(),
                "somehashedrelease",
                "WEBDL-720p",
                "Mercury"
            },
            new object[]
            {
                @"C:\Test\0e895c37245186812cb08aab1529cf8ee389dd05\Some.Hashed.Release.S01E01.720p.WEB-DL.AAC2.0.H.264-Mercury.mkv".AsOsAgnostic(),
                "somehashedrelease",
                "WEBDL-720p",
                "Mercury"
            },
            new object[]
            {
                @"C:\Test\Some.Hashed.Release.S01E01.720p.WEB-DL.AAC2.0.H.264-Mercury.mkv\yrucreM-462.H.0.2CAA.LD-BEW.p027.10E10S.esaeleR.dehsaH.emoS.mkv".AsOsAgnostic(),
                "somehashedrelease",
                "WEBDL-720p",
                "Mercury"
            },
            new object[]
            {
                @"C:\Test\Weeds.S01E10.DVDRip.XviD-NZBgeek\AHFMZXGHEWD660.mkv".AsOsAgnostic(),
                "weeds",
                "DVD",
                "NZBgeek"
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
