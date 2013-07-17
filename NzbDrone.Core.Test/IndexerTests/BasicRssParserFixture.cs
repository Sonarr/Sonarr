using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class BasicRssParserFixture : CoreTest<BasicRssParser>
    {

        [TestCase("Castle.2009.S01E14.English.HDTV.XviD-LOL", "LOL")]
        [TestCase("Castle 2009 S01E14 English HDTV XviD LOL", "LOL")]
        [TestCase("Acropolis Now S05 EXTRAS DVDRip XviD RUNNER", "RUNNER")]
        [TestCase("Punky.Brewster.S01.EXTRAS.DVDRip.XviD-RUNNER", "RUNNER")]
        [TestCase("2020.NZ.2011.12.02.PDTV.XviD-C4TV", "C4TV")]
        [TestCase("The.Office.S03E115.DVDRip.XviD-OSiTV", "OSiTV")]
        public void parse_releaseGroup(string title, string expected)
        {
            BasicRssParser.ParseReleaseGroup(title).Should().Be(expected);
        }


        [TestCase("5.64 GB", 6055903887)]
        [TestCase("5.54 GiB", 5948529705)]
        [TestCase("398.62 MiB", 417983365)]
        [TestCase("7,162.1MB", 7510006170)]
        [TestCase("162.1MB", 169974170)]
        [TestCase("398.62 MB", 417983365)]
        [TestCase("845 MB", 886046720)]
        public void parse_size(string sizeString, long expectedSize)
        {
            var result = BasicRssParser.GetReportSize(sizeString);

            result.Should().Be(expectedSize);
        }

    }
}