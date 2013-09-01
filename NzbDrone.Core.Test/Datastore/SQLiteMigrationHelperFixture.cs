using System.Collections.Generic;
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

            _subject.CreateTable("Series_New", columns.Values, new List<SQLiteMigrationHelper.SQLiteIndex>());

            var newColumns = _subject.GetColumns("Series_New");

            newColumns.Values.Should().HaveSameCount(columns.Values);
            newColumns.Should().NotContainKey("Title");
        }


        [Test]
        public void should_be_able_to_transfer_empty_tables()
        {
            var columns = _subject.GetColumns("Series");
            columns.Remove("Title");

            _subject.CreateTable("Series_New", columns.Values, new List<SQLiteMigrationHelper.SQLiteIndex>());


            _subject.CopyData("Series", "Series_New", columns.Values);
        }

        [Test]
        public void should_transfer_table_with_data()
        {
            var originalEpisodes = Builder<Episode>.CreateListOfSize(10).BuildListOfNew();

            Mocker.Resolve<EpisodeRepository>().InsertMany(originalEpisodes);

            var columns = _subject.GetColumns("Episodes");
            columns.Remove("Title");

            _subject.CreateTable("Episodes_New", columns.Values, new List<SQLiteMigrationHelper.SQLiteIndex>());

            _subject.CopyData("Episodes", "Episodes_New", columns.Values);

            _subject.GetRowCount("Episodes_New").Should().Be(originalEpisodes.Count);
        }

        [Test]
        public void should_read_existing_indexes()
        {
            var indexes = _subject.GetIndexes("QualitySizes");

            indexes.Should().NotBeEmpty();

            indexes.Should().OnlyContain(c => c != null);
            indexes.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Column));
            indexes.Should().OnlyContain(c => c.Table == "QualitySizes");
            indexes.Should().OnlyContain(c => c.Unique);
        }

        [Test]
        public void should_add_indexes_when_creating_new_table()
        {
            var columns = _subject.GetColumns("QualitySizes");
            var indexes = _subject.GetIndexes("QualitySizes");


            _subject.CreateTable("QualityB", columns.Values, indexes);


            var newIndexes = _subject.GetIndexes("QualityB");

            newIndexes.Should().HaveSameCount(indexes);
            newIndexes.Should().BeEquivalentTo(columns);
        }
    }
}