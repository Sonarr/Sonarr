using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FluentAssertions;
using Migrator.Framework;
using Migrator.Providers.SQLite;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using SubSonic.DataProviders;
using SubSonic.Repository;
using SubSonic.Schema;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class RepositoryProviderTest : TestBase
    {
        [Test]
        public void Get_Assembly_repos()
        {
            var provider = new RepositoryProvider();
            var types = provider.GetRepositoryTypes();

            types.Should().Contain(typeof(Config));
            types.Should().Contain(typeof(Episode));
            types.Should().Contain(typeof(EpisodeFile));
            types.Should().Contain(typeof(ExternalNotificationSetting));
            types.Should().Contain(typeof(History));
            types.Should().Contain(typeof(IndexerSetting));
            types.Should().Contain(typeof(JobSetting));
            types.Should().Contain(typeof(RootDir));
            types.Should().Contain(typeof(Series));
            types.Should().Contain(typeof(QualityProfile));

            types.Should().NotContain(typeof(QualityTypes));


        }





        [Test]
        public void Get_table_columns()
        {
            var provider = new RepositoryProvider();
            var typeTable = provider.GetSchemaFromType(typeof(TestRepoType));

            Assert.IsNotNull(typeTable.Columns);

            typeTable.Columns.Should().HaveCount(3);
            Assert.AreEqual("TestRepoTypes", typeTable.Name);
        }

        [Test]
        public void ConvertToMigratorColumn()
        {
            var provider = new RepositoryProvider();

            var subsonicColumn = new DatabaseColumn
                          {
                              Name = "Name",
                              DataType = DbType.Boolean,
                              IsPrimaryKey = true,
                              IsNullable = true
                          };

            var migColumn = provider.ConvertToMigratorColumn(subsonicColumn);

            Assert.IsTrue(migColumn.IsPrimaryKey);
            Assert.AreEqual(ColumnProperty.Null | ColumnProperty.PrimaryKey, migColumn.ColumnProperty);
        }


        [Test]
        public void GetDbColumns()
        {
            string connectionString = "Data Source=" + Guid.NewGuid() + ".db;Version=3;New=True";
            var dbProvider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");
            var repo = new SimpleRepository(dbProvider, SimpleRepositoryOptions.RunMigrations);
            var sqliteDatabase = new SQLiteTransformationProvider(new SQLiteDialect(), connectionString);

            repo.Add(new TestRepoType() { Value = "Dummy" });

            var repositoryProvider = new RepositoryProvider();
            var columns = repositoryProvider.GetColumnsFromDatabase(sqliteDatabase, "TestRepoTypes");

            columns.Should().HaveCount(3);

        }


        [Test]
        public void DeleteColumns()
        {
            string connectionString = "Data Source=" + Guid.NewGuid() + ".db;Version=3;New=True";
            var dbProvider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");
            var sqliteDatabase = new SQLiteTransformationProvider(new SQLiteDialect(), connectionString);
            var repo = new SimpleRepository(dbProvider, SimpleRepositoryOptions.RunMigrations);

            repo.Add(new TestRepoType() { Value = "Dummy" });

            var repositoryProvider = new RepositoryProvider();
            var typeSchema = repositoryProvider.GetSchemaFromType(typeof(TestRepoType2));
            var columns = repositoryProvider.GetColumnsFromDatabase(sqliteDatabase, "TestRepoTypes");


            var deletedColumns = repositoryProvider.GetDeletedColumns(typeSchema, columns);


            deletedColumns.Should().HaveCount(1);
            Assert.AreEqual("NewName", deletedColumns[0].Name.Trim('[', ']'));
        }


        [Test]
        public void NewColumns()
        {
            string connectionString = "Data Source=" + Guid.NewGuid() + ".db;Version=3;New=True";
            var dbProvider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");
            var repo = new SimpleRepository(dbProvider, SimpleRepositoryOptions.RunMigrations);
            var sqliteDatabase = new SQLiteTransformationProvider(new SQLiteDialect(), connectionString);

            repo.Add(new TestRepoType2() { Value = "dummy" });

            var repositoryProvider = new RepositoryProvider();
            var typeSchema = repositoryProvider.GetSchemaFromType(typeof(TestRepoType));
            var columns = repositoryProvider.GetColumnsFromDatabase(sqliteDatabase, "TestRepoType2s");


            var deletedColumns = repositoryProvider.GetNewColumns(typeSchema, columns);


            deletedColumns.Should().HaveCount(1);
            Assert.AreEqual("NewName", deletedColumns[0].Name.Trim('[', ']'));
        }

    }


    public class TestRepoType
    {
        [SubSonicPrimaryKey]
        public int TestId { get; set; }

        [SubSonicColumnNameOverride("NewName")]
        public Boolean BaddBoolean { get; set; }


        public string Value { get; set; }

        [SubSonicIgnore]
        public Boolean BaddBooleanIgnored { get; set; }
    }


    public class TestRepoType2
    {
        [SubSonicPrimaryKey]
        public int TestId { get; set; }

        public string Value { get; set; }

        [SubSonicIgnore]
        public Boolean BaddBooleanIgnored { get; set; }
    }
}
