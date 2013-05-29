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
            var callsToFetch = new List<DateTime>();

            Mocker.GetMock<IFetchFeedFromIndexers>().Setup(c => c.FetchRss(It.IsAny<IIndexer>()))
                .Returns(new List<ReportInfo>())
                  .Callback((() =>
                      {
                          Thread.Sleep(2000);
                          Console.WriteLine(DateTime.Now);
                          callsToFetch.Add(DateTime.Now);
                      }));

            Mocker.GetMock<IIndexerService>().Setup(c => c.GetAvailableIndexers()).Returns(_indexers);

            Subject.Fetch();


            var first = callsToFetch.Min();
            var last = callsToFetch.Max();

            (last - first).Should().BeLessThan(TimeSpan.FromSeconds(1));
        }
    }
}