using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class BasicRssParserFixture : CoreTest<RssParserBase>
    {
        [TestCase("5.64 GB", 6055903887)]
        [TestCase("5.54 GiB", 5948529705)]
        [TestCase("398.62 MiB", 417983365)]
        [TestCase("7,162.1MB", 7510006170)]
        [TestCase("162.1MB", 169974170)]
        [TestCase("398.62 MB", 417983365)]
        [TestCase("845 MB", 886046720)]
        public void parse_size(string sizeString, long expectedSize)
        {
            var result = RssParserBase.ParseSize(sizeString);

            result.Should().Be(expectedSize);
        }

    }
}