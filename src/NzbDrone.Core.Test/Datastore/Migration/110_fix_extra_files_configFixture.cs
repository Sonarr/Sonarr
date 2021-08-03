using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class fix_extra_files_configFixture : MigrationTest<fix_extra_files_config>
    {
        [Test]
        public void should_not_update_importextrafiles_disabled()
        {
            var db = WithMigrationTestDb();

            var itemEnabled = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'importextrafiles'");
            itemEnabled.Should().BeNull();
        }

        [Test]
        public void should_fix_importextrafiles_if_wrong()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "importextrafiles",
                    Value = 1
                });
            });

            var itemEnabled = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'importextrafiles'");
            itemEnabled.Should().Be("True");
        }

        [Test]
        public void should_fill_in_default_extensions()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "importextrafiles",
                    Value = "False"
                });

                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "extrafileextensions",
                    Value = ""
                });
            });

            var itemEnabled = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'importextrafiles'");
            itemEnabled.Should().Be("False");

            var itemExtensions = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'extrafileextensions'");
            itemExtensions.Should().Be("srt");
        }

        [Test]
        public void should_not_fill_in_default_extensions()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "importextrafiles",
                    Value = "True"
                });

                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "extrafileextensions",
                    Value = ""
                });
            });

            var itemEnabled = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'importextrafiles'");
            itemEnabled.Should().Be("True");

            var itemExtensions = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'extrafileextensions'");
            itemExtensions.Should().Be("");
        }

        [Test]
        public void should_not_fill_in_default_extensions_if_not_defined()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "importextrafiles",
                    Value = "False"
                });
            });

            var itemEnabled = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'importextrafiles'");
            itemEnabled.Should().Be("False");

            var itemExtensions = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'extrafileextensions'");
            itemExtensions.Should().BeNull();
        }

        [Test]
        public void should_not_fill_in_default_extensions_if_already_defined()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "importextrafiles",
                    Value = "False"
                });

                c.Insert.IntoTable("Config").Row(new
                {
                    Key = "extrafileextensions",
                    Value = "sub"
                });
            });

            var itemEnabled = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'importextrafiles'");
            itemEnabled.Should().Be("False");

            var itemExtensions = db.QueryScalar<string>("SELECT Value FROM Config WHERE Key = 'extrafileextensions'");
            itemExtensions.Should().Be("sub");
        }
    }
}
