using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Wombles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class IndexerCheckFixture : CoreTest<IndexerCheck>
    {
        [Test]
        public void should_return_error_when_not_indexers_are_enabled()
        {
            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.GetAvailableProviders())
                  .Returns(new List<IIndexer>());

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_warning_when_only_enabled_indexer_is_wombles()
        {
            var indexer = Mocker.GetMock<IIndexer>();
            indexer.SetupGet(s => s.SupportsSearching).Returns(false);

            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.GetAvailableProviders())
                  .Returns(new List<IIndexer>{indexer.Object});

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_null_when_multiple_indexers_are_enabled()
        {
            var indexer1 = Mocker.GetMock<IIndexer>();
            indexer1.SetupGet(s => s.SupportsSearching).Returns(true);

            var indexer2 = Mocker.GetMock<Wombles>();
            indexer2.SetupGet(s => s.SupportsSearching).Returns(false);

            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.GetAvailableProviders())
                  .Returns(new List<IIndexer> { indexer1.Object, indexer2.Object });

            Subject.Check().Should().BeNull();
        }

        [Test]
        public void should_return_null_when_indexer_supports_searching()
        {
            var indexer1 = Mocker.GetMock<IIndexer>();
            indexer1.SetupGet(s => s.SupportsSearching).Returns(true);

            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.GetAvailableProviders())
                  .Returns(new List<IIndexer> { indexer1.Object });

            Subject.Check().Should().BeNull();
        }
    }
}
