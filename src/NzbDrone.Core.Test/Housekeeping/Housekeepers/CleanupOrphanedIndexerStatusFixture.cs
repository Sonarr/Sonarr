using System.Threading.Tasks;
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

        private async Task GivenIndexer()
        {
            await Db.InsertAsync(_indexer);
        }

        [Test]
        public async Task should_delete_orphaned_indexerstatus()
        {
            var status = Builder<IndexerStatus>.CreateNew()
                                               .With(h => h.ProviderId = _indexer.Id)
                                               .BuildNew();
            await Db.InsertAsync(status);

            Subject.Clean();
            var indexerStatuses = await GetAllStoredModelsAsync();
            indexerStatuses.Should().BeEmpty();
        }

        [Test]
        public async Task should_not_delete_unorphaned_indexerstatus()
        {
            await GivenIndexer();

            var status = Builder<IndexerStatus>.CreateNew()
                                               .With(h => h.ProviderId = _indexer.Id)
                                               .BuildNew();
            await Db.InsertAsync(status);

            Subject.Clean();
            var indexerStatuses = await GetAllStoredModelsAsync();
            indexerStatuses.Should().HaveCount(1);
            indexerStatuses.Should().Contain(h => h.ProviderId == _indexer.Id);
        }
    }
}
