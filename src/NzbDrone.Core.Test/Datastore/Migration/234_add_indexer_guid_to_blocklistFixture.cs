using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_indexer_guid_to_blocklistFixture : MigrationTest<add_indexer_guid_to_blocklist>
    {
        [Test]
        public void should_add_nullable_indexer_guid_column_to_blocklist()
        {
            using var db = WithDapperMigrationTestDb();

            var columns = db.Query<ColumnDefinition234>("PRAGMA table_info(\"Blocklist\")").ToList();
            var indexerGuid = columns.Single(c => c.Name == "IndexerGuid");

            indexerGuid.NotNull.Should().Be(0);
        }
    }

    public class ColumnDefinition234
    {
        public string Name { get; set; }
        public int NotNull { get; set; }
    }
}
