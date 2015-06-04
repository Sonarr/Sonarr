using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class BasicRssParserFixture : CoreTest<RssParser>
    {
        [TestCase("5.64 GB", 6055903887)]
        [TestCase("5.54 GiB", 5948529705)]
        [TestCase("398.62 MiB", 417983365)]
        [TestCase("7,162.1MB", 7510006170)]
        [TestCase("162.1MB", 169974170)]
        [TestCase("398.62 MB", 417983365)]
        [TestCase("845 MB", 886046720)]
        [TestCase("7,162,100.0KB", 7333990400)]
        [TestCase("12341234", 12341234)]
        public void should_parse_size(string sizeString, long expectedSize)
        {
            var result = RssParser.ParseSize(sizeString, true);

            result.Should().Be(expectedSize);
        }

        [TestCase("100 Kbps")]
        [TestCase("100 Kb/s")]
        [TestCase(" 12341234")]
        [TestCase("12341234 other")]
        public void should_not_parse_size(string sizeString)
        {
            var result = RssParser.ParseSize(sizeString, true);

            result.Should().Be(0);
        }

    }
}