using FluentAssertions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.NzbClub;
using NzbDrone.Core.Test.Framework;
using NUnit.Framework;

namespace NzbDrone.Core.Test.IndexerTests.IntegrationTests
{
    public class NzbClubIntegrationTests : CoreTest<FetchFeedService>
    {
        [Test]
        public void should_be_able_to_fetch_rss()
        {
            UseRealHttp();

            var indexer = new NzbClub();

            var result = Subject.FetchRss(indexer);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Title));
            result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.NzbUrl));

            //TODO: uncomment these after moving to restsharp for rss
            //result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.NzbInfoUrl));
            //result.Should().OnlyContain(c => c.Size > 0);

        }

    }
}