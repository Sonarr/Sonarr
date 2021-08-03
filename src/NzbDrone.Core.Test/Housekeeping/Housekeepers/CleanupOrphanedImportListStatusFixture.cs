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

        private void GivenIndexer()
        {
            Db.Insert(_importList);
        }

        [Test]
        public void should_delete_orphaned_indexerstatus()
        {
            var status = Builder<ImportListStatus>.CreateNew()
                                                  .With(h => h.ProviderId = _importList.Id)
                                                  .BuildNew();
            Db.Insert(status);

            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_indexerstatus()
        {
            GivenIndexer();

            var status = Builder<ImportListStatus>.CreateNew()
                                                  .With(h => h.ProviderId = _importList.Id)
                                                  .BuildNew();
            Db.Insert(status);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(h => h.ProviderId == _importList.Id);
        }
    }
}
