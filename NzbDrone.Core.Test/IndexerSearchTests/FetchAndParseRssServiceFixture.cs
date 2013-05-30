using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    public class NzbSearchServiceFixture : CoreTest<NzbSearchService>
    {
        private List<IIndexer> _indexers;

        private Series _searchTargetSeries;

        [SetUp]
        public void Setup()
        {

            _searchTargetSeries = Builder<Series>.CreateNew().BuildNew();

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

            Mocker.GetMock<ISeriesService>().Setup(c => c.GetSeries(It.IsAny<int>()))
                  .Returns(_searchTargetSeries);

        }

        [Test]
        public void should_call_fetch_on_all_indexers_at_the_same_time()
        {

            var counter = new ConcurrencyCounter(_indexers.Count);

            Mocker.GetMock<IFetchFeedFromIndexers>().Setup(c => c.Fetch(It.IsAny<IIndexer>(), It.IsAny<SingleEpisodeSearchDefinition>()))
                  .Returns(new List<ReportInfo>())
                  .Callback((() => counter.SimulateWork(500)));

            Mocker.GetMock<IIndexerService>().Setup(c => c.GetAvailableIndexers()).Returns(_indexers);

            Mocker.GetMock<IMakeDownloadDecision>()
                  .Setup(c => c.GetSearchDecision(It.IsAny<IEnumerable<ReportInfo>>(), It.IsAny<SearchDefinitionBase>()))
                  .Returns(new List<DownloadDecision>());

            Subject.SearchSingle(0, 0, 0);

            counter.WaitForAllItems();

            counter.MaxThreads.Should().Be(_indexers.Count);
        }
    }
}