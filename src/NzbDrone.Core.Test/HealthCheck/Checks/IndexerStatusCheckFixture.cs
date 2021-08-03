using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class IndexerStatusCheckFixture : CoreTest<IndexerStatusCheck>
    {
        private List<IIndexer> _indexers = new List<IIndexer>();
        private List<IndexerStatus> _blockedIndexers = new List<IndexerStatus>();

        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IIndexerFactory>()
                  .Setup(v => v.GetAvailableProviders())
                  .Returns(_indexers);

            Mocker.GetMock<IIndexerStatusService>()
                   .Setup(v => v.GetBlockedProviders())
                   .Returns(_blockedIndexers);
        }

        private Mock<IIndexer> GivenIndexer(int id, double backoffHours, double failureHours)
        {
            var mockIndexer = new Mock<IIndexer>();
            mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition { Id = id });
            mockIndexer.SetupGet(s => s.SupportsSearch).Returns(true);

            _indexers.Add(mockIndexer.Object);

            if (backoffHours != 0.0)
            {
                _blockedIndexers.Add(new IndexerStatus
                    {
                        ProviderId = id,
                        InitialFailure = DateTime.UtcNow.AddHours(-failureHours),
                        MostRecentFailure = DateTime.UtcNow.AddHours(-0.1),
                        EscalationLevel = 5,
                        DisabledTill = DateTime.UtcNow.AddHours(backoffHours)
                    });
            }

            return mockIndexer;
        }

        [Test]
        public void should_not_return_error_when_no_indexers()
        {
            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_warning_if_indexer_unavailable()
        {
            GivenIndexer(1, 2.0, 4.0);
            GivenIndexer(2, 0.0, 0.0);

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_error_if_all_indexers_unavailable()
        {
            GivenIndexer(1, 2.0, 4.0);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_warning_if_few_indexers_unavailable()
        {
            GivenIndexer(1, 2.0, 4.0);
            GivenIndexer(2, 2.0, 4.0);
            GivenIndexer(3, 0.0, 0.0);

            Subject.Check().ShouldBeWarning();
        }
    }
}
