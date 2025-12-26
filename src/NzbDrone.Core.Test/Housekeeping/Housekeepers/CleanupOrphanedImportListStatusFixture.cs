using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedImportListFixture : DbTest<CleanupOrphanedImportListStatus, ImportListStatus>
    {
        private ImportListDefinition _importList;

        [SetUp]
        public void Setup()
        {
            _importList = Builder<ImportListDefinition>.CreateNew()
                                                 .BuildNew();
        }

        private async Task GivenIndexer()
        {
            await Db.InsertAsync(_importList);
        }

        [Test]
        public async Task should_delete_orphaned_indexerstatus()
        {
            var status = Builder<ImportListStatus>.CreateNew()
                                                  .With(h => h.ProviderId = _importList.Id)
                                                  .BuildNew();
            await Db.InsertAsync(status);

            Subject.Clean();
            var importListStatuses = await GetAllStoredModelsAsync();
            importListStatuses.Should().BeEmpty();
        }

        [Test]
        public async Task should_not_delete_unorphaned_indexerstatus()
        {
            await GivenIndexer();

            var status = Builder<ImportListStatus>.CreateNew()
                                                  .With(h => h.ProviderId = _importList.Id)
                                                  .BuildNew();
            await Db.InsertAsync(status);

            Subject.Clean();
            var importListStatuses = await GetAllStoredModelsAsync();
            importListStatuses.Should().HaveCount(1);
            importListStatuses.Should().Contain(h => h.ProviderId == _importList.Id);
        }
    }
}
