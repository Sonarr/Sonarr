using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class UrlFixture : CoreTest
    {
        [TestCase("[www.test.com] - Series.S03E14.720p.HDTV.X264-DIMENSION", "Series")]
        [TestCase("test.net - Series.S03E14.720p.HDTV.X264-DIMENSION", "Series")]
        [TestCase("[www.test-hyphen.com] - Series.S03E14.720p.HDTV.X264-DIMENSION", "Series")]
        [TestCase("www.test123.org - Series.S03E14.720p.HDTV.X264-DIMENSION", "Series")]
        [TestCase("[test.co.uk] - Series.S03E14.720p.HDTV.X264-DIMENSION", "Series")]
        [TestCase("www.test-hyphen.net.au - Series.S03E14.720p.HDTV.X264-DIMENSION", "Series")]
        [TestCase("[www.test123.co.nz] - Series.S03E14.720p.HDTV.X264-DIMENSION", "Series")]
        [TestCase("test-hyphen123.org.au - Series.S03E14.720p.HDTV.X264-DIMENSION", "Series")]
        [TestCase("[www.test123.de] - Mad Series - Season 1 [Bluray720p]", "Mad Series")]
        [TestCase("www.test-hyphen.de - Mad Series - Season 1 [Bluray1080p]", "Mad Series")]
        [TestCase("[test-hyphen123.co.za] - The Daily Series - 2023-05-26", "The Daily Series")]
        [TestCase("www.test123.co.za - The Series Bros. (2006) - S01E01", "The Series Bros. (2006)")]
        [TestCase("[www.test-hyphen.ca] - Series (2011) S01", "Series (2011)")]
        [TestCase("test123.ca - Series Time S02 720p HDTV x264 CRON", "Series Time")]
        [TestCase("[www.test-hyphen123.co.za] - Series Title S01E01", "Series Title")]

        public void should_not_parse_url_in_name(string postTitle, string title)
        {
            var result = Parser.Parser.ParseSeriesName(postTitle).CleanSeriesTitle();
            result.Should().Be(title.CleanSeriesTitle());
        }

        [TestCase("Series.2009.S01E14.English.HDTV.XviD-LOL[www.abb.com]", "LOL")]
        [TestCase("Series 2009 S01E14 English HDTV XviD LOL[www.academy.org]", null)]
        [TestCase("Series Now S05 EXTRAS DVDRip XviD RUNNER[www.aetna.net]", null)]
        [TestCase("Series.Title.S01.EXTRAS.DVDRip.XviD-RUNNER[www.alfaromeo.io]", "RUNNER")]
        [TestCase("2020.Series.2011.12.02.PDTV.XviD-C4TV[rarbg.to]", "C4TV")]
        [TestCase("Series.Title.S01E14.English.HDTV.XviD-LOL[www.abbott.gov]", "LOL")]
        [TestCase("Series 2020 S01E14 English HDTV XviD LOL[www.actor.org]", null)]
        [TestCase("Series Live S05 EXTRAS DVDRip XviD RUNNER[www.agency.net]", null)]
        [TestCase("Series.Title.S02.EXTRAS.DVDRip.XviD-RUNNER[www.airbus.io]", "RUNNER")]
        [TestCase("2021.Series.2012.12.02.PDTV.XviD-C4TV[rarbg.to]", "C4TV")]
        [TestCase("Series.2020.S01E14.English.HDTV.XviD-LOL[www.afl.com]", "LOL")]
        [TestCase("Series 2021 S01E14 English HDTV XviD LOL[www.adult.org]", null)]
        [TestCase("Series Future S05 EXTRAS DVDRip XviD RUNNER[www.allstate.net]", null)]
        [TestCase("Series.Title.S03.EXTRAS.DVDRip.XviD-RUNNER[www.ally.io]", "RUNNER")]
        [TestCase("2022.Series.2013.12.02.PDTV.XviD-C4TV[rarbg.to]", "C4TV")]

        public void should_not_parse_url_in_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }
    }
}
