using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class FetchAndParseRssServiceFixture : CoreTest<FetchAndParseRssService>
    {
        private List<IIndexer> _indexers;

        [SetUp]
        public void Setup()
        {
            _indexers = new List<IIndexer>();

            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());
            _indexers.Add(new Newznab());

            Mocker.SetConstant<IEnumerable<IIndexer>>(_indexers);

        }

        [Test]
        [Explicit]
        public void should_call_fetch_on_all_indexers_at_the_same_time()
        {

            var counter = new ConcurrencyCounter(_indexers.Count);

            Mocker.GetMock<IFetchFeedFromIndexers>().Setup(c => c.FetchRss(It.IsAny<IIndexer>()))
                .Returns(new List<ReportInfo>())
                  .Callback((() => counter.SimulateWork(500)));

            Mocker.GetMock<IIndexerService>().Setup(c => c.GetAvailableIndexers()).Returns(_indexers);

            Subject.Fetch();

            counter.WaitForAllItems();

            counter.MaxThreads.Should().Be(_indexers.Count);
        }
    }
}