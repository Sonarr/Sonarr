using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class import_extra_files_configFixture : MigrationTest<import_extra_files>
    {
        [Test]
        public void should_not_insert_if_missing()
        {
            var db = WithMigrationTestDb();

            var items = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'importextrafiles'");
            items.Should().BeNull();
        }

        [Test]
        public void should_not_insert_if_empty()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "extrafileextensions",
                    Value = ""
                });
            });

            var items = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'importextrafiles'");
            items.Should().BeNull();
        }

        [Test]
        public void should_insert_True_if_configured()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "extrafileextensions",
                    Value = "srt"
                });
            });

            var items = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'importextrafiles'");
            items.Should().Be("True");
        }
    }
}
