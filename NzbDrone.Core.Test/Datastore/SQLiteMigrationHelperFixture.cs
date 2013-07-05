using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class SQLiteMigrationHelperFixture : DbTest
    {
        private SQLiteMigrationHelper _subject;

        [SetUp]
        public void SetUp()
        {
            _subject = Mocker.Resolve<SQLiteMigrationHelper>();
        }



        [Test]
        public void should_parse_existing_columns()
        {
            var columns = _subject.GetColumns("Series");

            columns.Should().NotBeEmpty();

            columns.Values.Should().NotContain(c => string.IsNullOrWhiteSpace(c.Name));
            columns.Values.Should().NotContain(c => string.IsNullOrWhiteSpace(c.Schema));
        }

        [Test]
        public void should_create_table_from_column_list()
        {
            var columns = _subject.GetColumns("Series");
            columns.Remove("Title");

            _subject.CreateTable("Series_New", columns.Values);

            var newColumns = _subject.GetColumns("Series_New");

            newColumns.Values.Should().HaveSameCount(columns.Values);
            newColumns.Should().NotContainKey("Title");
        }

        [Test]
        public void should_get_zero_count_on_empty_table()
        {
            _subject.GetRowCount("Series").Should().Be(0);
        }


        [Test]
        public void should_be_able_to_transfer_empty_tables()
        {
            var columns = _subject.GetColumns("Series");
            columns.Remove("Title");

            _subject.CreateTable("Series_New", columns.Values);


            _subject.CopyData("Series", "Series_New", columns.Values);
        }

        [Test]
        public void should_transfer_table_with_data()
        {
            var originalEpisodes = Builder<Episode>.CreateListOfSize(10).BuildListOfNew();

            Mocker.Resolve<EpisodeRepository>().InsertMany(originalEpisodes);

            var columns = _subject.GetColumns("Episodes");
            columns.Remove("Title");

            _subject.CreateTable("Episodes_New", columns.Values);

            _subject.CopyData("Episodes", "Episodes_New", columns.Values);

            _subject.GetRowCount("Episodes_New").Should().Be(originalEpisodes.Count);
        }
    }
}