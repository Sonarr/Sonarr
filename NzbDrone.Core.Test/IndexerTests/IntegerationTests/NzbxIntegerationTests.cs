using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Nzbx;
using NzbDrone.Core.Test.Framework;
using NUnit.Framework;

namespace NzbDrone.Core.Test.IndexerTests.IntegerationTests
{
    public class NzbxIntegerationTests : CoreTest<FetchFeedService>
    {

        [Test]
        public void should_be_able_to_fetch_rss()
        {
            UseRealHttp();

            var indexer = new Nzbx();

            Subject.FetchRss(indexer);
        }

    }
}