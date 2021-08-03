using System.Data;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Test.Datastore.SqliteSchemaDumperTests
{
    [TestFixture]
    public class SqliteSchemaDumperFixture
    {
        public SqliteSchemaDumper Subject { get; private set; }

        [SetUp]
        public void Setup()
        {
            Subject = new SqliteSchemaDumper(null);
        }

        [TestCase(@"CREATE TABLE TestTable (MyId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)", "TestTable", "MyId")]
        [TestCase(@"CREATE TABLE ""TestTable"" (""MyId"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)", "TestTable", "MyId")]
        [TestCase(@"CREATE TABLE [TestTable] ([MyId] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)", "TestTable", "MyId")]
        [TestCase(@"CREATE TABLE `TestTable` (`MyId` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)", "TestTable", "MyId")]
        [TestCase(@"CREATE TABLE 'TestTable' ('MyId' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)", "TestTable", "MyId")]
        [TestCase(@"CREATE TABLE ""Test """"Table"" (""My""""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)", "Test \"Table", "My\"Id")]
        [TestCase(@"CREATE TABLE [Test Table] ([My Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT)", "Test Table", "My Id")]
        [TestCase(@" CREATE  TABLE  `Test ``Table`  ( `My``  Id`  INTEGER  NOT  NULL  PRIMARY  KEY  AUTOINCREMENT ) ", "Test `Table", "My`  Id")]
        [TestCase(@"CREATE TABLE TestTable (MyId INTEGER NOT NULL,  PRIMARY KEY(""MyId"" AUTOINCREMENT))", "TestTable", "MyId")]
        public void should_parse_table_language_flavors(string sql, string tableName, string columnName)
        {
            var result = Subject.ReadTableSchema(sql);

            result.Name.Should().Be(tableName);
            result.Columns.Count.Should().Be(1);
            result.Columns.First().Name.Should().Be(columnName);
            result.Columns.First().IsIdentity.Should().BeTrue();
        }

        [TestCase(@"CREATE INDEX TestIndex ON TestTable (MyId)", "TestIndex", "TestTable", "MyId")]
        [TestCase(@"CREATE INDEX ""TestIndex"" ON ""TestTable"" (""MyId"" ASC)", "TestIndex", "TestTable", "MyId")]
        [TestCase(@"CREATE INDEX 'TestIndex' ON 'TestTable' ('MyId' ASC)", "TestIndex", "TestTable", "MyId")]
        [TestCase(@"CREATE INDEX [TestIndex] ON ""TestTable"" ([MyId] DESC)", "TestIndex", "TestTable", "MyId")]
        [TestCase(@"CREATE INDEX `TestIndex`  ON `TestTable`  (`MyId` COLLATE abc ASC)", "TestIndex", "TestTable", "MyId")]
        [TestCase(@"CREATE INDEX ""Test """"Index"" ON ""TestTable"" (""My""""Id"" ASC)", "Test \"Index", "TestTable", "My\"Id")]
        [TestCase(@"CREATE INDEX [Test Index] ON [TestTable] ([My Id]) ", "Test Index", "TestTable", "My Id")]
        [TestCase(@" CREATE  INDEX  `Test ``Index` ON ""TestTable""  ( `My``  Id`  ASC) ", "Test `Index", "TestTable", "My`  Id")]
        public void should_parse_index_language_flavors(string sql, string indexName, string tableName, string columnName)
        {
            var result = Subject.ReadIndexSchema(sql);

            result.Name.Should().Be(indexName);
            result.TableName.Should().Be(tableName);
            result.Columns.Count.Should().Be(1);
            result.Columns.First().Name.Should().Be(columnName);
        }

        [TestCase(@"CREATE TABLE TestTable (MyId)")]
        [TestCase(@"CREATE TABLE TestTable (MyId  NOT NULL PRIMARY KEY AUTOINCREMENT)")]
        [TestCase("CREATE TABLE TestTable\r\n(\t`MyId`\t NOT NULL PRIMARY KEY AUTOINCREMENT\n)")]
        public void should_parse_column_attributes(string sql)
        {
            var result = Subject.ReadTableSchema(sql);

            result.Name.Should().Be("TestTable");
            result.Columns.Count.Should().Be(1);
            result.Columns.First().Name.Should().Be("MyId");
            result.Columns.First().Type.Should().BeNull();
        }

        [Test]
        public void should_ignore_unknown_symbols()
        {
            var result = Subject.ReadTableSchema("CREATE TABLE TestTable (MyId INTEGER DEFAULT 10 CHECK (Some weir +1e3 expression), CONSTRAINT NULL, MyCol INTEGER)");

            result.Name.Should().Be("TestTable");
            result.Columns.Count.Should().Be(2);
            result.Columns.First().Name.Should().Be("MyId");
            result.Columns.First().Type.Should().Be(DbType.Int64);
            result.Columns.Last().Name.Should().Be("MyCol");
            result.Columns.Last().Type.Should().Be(DbType.Int64);
        }
    }
}
