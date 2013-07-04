using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class SQLiteAlterFixture : DbTest
    {
        private SQLiteAlter Subject;

        [SetUp]
        public void SetUp()
        {
            var connection = Mocker.Resolve<IDatabase>().DataMapper.ConnectionString;
            Subject = new SQLiteAlter(connection);
        }



        [Test]
        public void should_parse_existing_columns()
        {
            var columns = Subject.GetColumns("Series");

            columns.Should().NotBeEmpty();

            columns.Values.Should().NotContain(c => string.IsNullOrWhiteSpace(c.Name));
            columns.Values.Should().NotContain(c => string.IsNullOrWhiteSpace(c.Schema));
        }

        [Test]
        public void should_create_table_from_column_list()
        {
            var columns = Subject.GetColumns("Series");
            columns.Remove("Title");

            Subject.CreateTable("Series_New", columns.Values);

            var newColumns = Subject.GetColumns("Series_New");

            newColumns.Values.Should().HaveSameCount(columns.Values);
            newColumns.Should().NotContainKey("Title");
        }
    }
}