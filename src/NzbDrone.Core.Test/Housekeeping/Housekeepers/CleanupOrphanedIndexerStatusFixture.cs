using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedIndexerStatusFixture : DbTest<CleanupOrphanedIndexerStatus, IndexerStatus>
    {
        private IndexerDefinition _indexer;

        [SetUp]
        public void Setup()
        {
            _indexer = Builder<IndexerDefinition>.CreateNew()
                                                 .BuildNew();
        }

        private void GivenIndexer()
        {
            Db.Insert(_indexer);
        }

        [Test]
        public void should_delete_orphaned_indexerstatus()
        {
            var status = Builder<IndexerStatus>.CreateNew()
                                               .With(h => h.ProviderId = _indexer.Id)
                                               .BuildNew();
            Db.Insert(status);

            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_indexerstatus()
        {
            GivenIndexer();

            var status = Builder<IndexerStatus>.CreateNew()
                                               .With(h => h.ProviderId = _indexer.Id)
                                               .BuildNew();
            Db.Insert(status);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(h => h.ProviderId == _indexer.Id);
        }
    }
}
