using NzbDrone.Core.Indexers.Nzbx;
using NzbDrone.Core.Test.Framework;
using NUnit.Framework;
using System.Linq;
using FluentAssertions;

namespace NzbDrone.Core.Test.IndexerTests.ParserTests
{
    public class NzbxParserFixture : CoreTest<NzbxParser>
    {
        [Test]
        public void should_be_able_to_parse_json()
        {
            var stream = OpenRead("Files", "Indexers", "Nzbx", "nzbx_recent.json");

            var result = Subject.Process(stream).ToList();

            result.Should().NotBeEmpty();
        }
    }
}