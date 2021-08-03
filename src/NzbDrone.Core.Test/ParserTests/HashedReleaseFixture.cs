using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
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
                "Some Hashed Release",
                Quality.WEBDL720p,
                "Mercury"
            },
            new object[]
            {
                @"C:\Test\0e895c37245186812cb08aab1529cf8ee389dd05\Some.Hashed.Release.S01E01.720p.WEB-DL.AAC2.0.H.264-Mercury.mkv".AsOsAgnostic(),
                "Some Hashed Release",
                Quality.WEBDL720p,
                "Mercury"
            },
            new object[]
            {
                @"C:\Test\Fake.Dir.S01E01-Test\yrucreM-462.H.0.2CAA.LD-BEW.p027.10E10S.esaeleR.dehsaH.emoS.mkv".AsOsAgnostic(),
                "Some Hashed Release",
                Quality.WEBDL720p,
                "Mercury"
            },
            new object[]
            {
                @"C:\Test\Fake.Dir.S01E01-Test\yrucreM-LN 1.5DD LD-BEW P0801 10E10S esaeleR dehsaH emoS.mkv".AsOsAgnostic(),
                "Some Hashed Release",
                Quality.WEBDL1080p,
                "Mercury"
            },
            new object[]
            {
                @"C:\Test\Title.S01E10.DVDRip.XviD-SONARR\AHFMZXGHEWD660.mkv".AsOsAgnostic(),
                "Title",
                Quality.DVD,
                "SONARR"
            },
            new object[]
            {
                @"C:\Test\Show Title.S02E12.1080p.BluRay.x264-SONARR\Backup_72023S02-12.mkv".AsOsAgnostic(),
                "Show Title",
                Quality.Bluray1080p,
                "SONARR"
            },
            new object[]
            {
                @"C:\Test\Title S04E08 Chupacabra 720p WEB-DL DD5 1 H 264-ECI\123.mkv".AsOsAgnostic(),
                "Title",
                Quality.WEBDL720p,
                "ECI"
            },
            new object[]
            {
                @"C:\Test\Title S04E08 Chupacabra 720p WEB-DL DD5 1 H 264-ECI\abc.mkv".AsOsAgnostic(),
                "Title",
                Quality.WEBDL720p,
                "ECI"
            },
            new object[]
            {
                @"C:\Test\Title S04E08 Chupacabra 720p WEB-DL DD5 1 H 264-ECI\b00bs.mkv".AsOsAgnostic(),
                "Title",
                Quality.WEBDL720p,
                "ECI"
            },
            new object[]
            {
                @"C:\Test\The.Show.Title.S02E23.720p.HDTV.x264-NZBgeek/cgajsofuejsa501.mkv".AsOsAgnostic(),
                "The Show Title",
                Quality.HDTV720p,
                "NZBgeek"
            },
            new object[]
            {
                @"C:\Test\Show.Title.S03E04.1080p.WEB-DL.DD5.1.H264-RARBG\170424_26.mkv".AsOsAgnostic(),
                "Show Title",
                Quality.WEBDL1080p,
                "RARBG"
            },
            new object[]
            {
                @"C:\Test\XxQVHK4GJMP3n2dLpmhW\XxQVHK4GJMP3n2dLpmhW\MKV\010E70S.nuF.fo.snoS.mkv".AsOsAgnostic(),
                "Sons of Fun",
                Quality.HDTV720p,
                null
            },
            new object[]
            {
                @"50E50S.denorD.mkv".AsOsAgnostic(),
                "Droned",
                Quality.HDTV720p,
                null
            },
            new object[]
            {
                @"C:\Test\XxQVHK4GJMP3n2dLpmhW\XxQVHK4GJMP3n2dLpmhW\50E50S.denorD.mkv".AsOsAgnostic(),
                "Droned",
                Quality.HDTV720p,
                null
            },
            new object[]
            {
                @"C:\Test\Series.Title.S02E24.720p.HDTV.H.264\abc.xyz.af6021c37f7852.mkv".AsOsAgnostic(),
                "Series Title",
                Quality.HDTV720p,
                null
            },
            new object[]
            {
                @"C:\Test\tv\Series.Title.S04E03.Episode.Name.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb\cwnOJYks5E2WP7zGuzPkdkRK3JkWw0.mkv".AsOsAgnostic(),
                "Series Title",
                Quality.WEBDL1080p,
                "NTb"
            },
            new object[]
            {
                @"C:\Test\tv\Series.Title.2017.S04E07.Episode.Name.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb/s2e2fYzx5xJhuBjHn5ZXE07Ebi.mkv".AsOsAgnostic(),
                "Series Title 2017",
                Quality.WEBDL1080p,
                "NTb"
            },
            new object[]
            {
                @"C:\Test\tv\Series.Title.S04E03.Episode.Name.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb\zM0vnVU1SoV4uwTihfzTHkYmJPBemgN3MqXG1fY.mkv".AsOsAgnostic(),
                "Series Title",
                Quality.WEBDL1080p,
                "NTb"
            },
            new object[]
            {
                @"C:\Test\tv\Series.Title.2019.S02E02.The.Episode.of.Names.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb/46XuzmawYQeUBwNdH2Hw2996.mkv".AsOsAgnostic(),
                "Series Title 2019",
                Quality.WEBDL1080p,
                "NTb"
            }
        };

        [Test]
        [TestCaseSource(nameof(HashedReleaseParserCases))]
        public void should_properly_parse_hashed_releases(string path, string title, Quality quality, string releaseGroup)
        {
            var result = Parser.Parser.ParsePath(path);
            result.SeriesTitle.Should().Be(title);
            result.Quality.Quality.Should().Be(quality);
            result.ReleaseGroup.Should().Be(releaseGroup);
        }
    }
}
