using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class IndexerSearchCheckFixture : CoreTest<IndexerSearchCheck>
    {
        private Mock<IIndexer> _indexerMock;

        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.GetAvailableProviders())
                  .Returns(new List<IIndexer>());

            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.SearchEnabled(It.IsAny<bool>()))
                  .Returns(new List<IIndexer>());
        }

        private void GivenIndexer(bool supportsRss, bool supportsSearch)
        {
            _indexerMock = Mocker.GetMock<IIndexer>();
            _indexerMock.SetupGet(s => s.SupportsRss).Returns(supportsRss);
            _indexerMock.SetupGet(s => s.SupportsSearch).Returns(supportsSearch);

            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.GetAvailableProviders())
                  .Returns(new List<IIndexer> { _indexerMock.Object });
        }

        private void GivenSearchEnabled()
        {
            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.SearchEnabled(It.IsAny<bool>()))
                  .Returns(new List<IIndexer> { _indexerMock.Object });
        }

        private void GivenSearchFiltered()
        {
            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.SearchEnabled(false))
                  .Returns(new List<IIndexer> { _indexerMock.Object });
        }

        [Test]
        public void should_return_warning_when_no_indexer_present()
        {
            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_warning_when_no_search_supported_indexer_present()
        {
            GivenIndexer(true, false);

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_ok_when_search_is_enabled()
        {
            GivenIndexer(false, true);
            GivenSearchEnabled();

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_warning_if_search_is_supported_but_disabled()
        {
            GivenIndexer(false, true);

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_filter_warning_if_search_is_enabled_but_filtered()
        {
            GivenIndexer(false, true);
            GivenSearchFiltered();

            Subject.Check().ShouldBeWarning("recent indexer errors");
        }
    }
}
