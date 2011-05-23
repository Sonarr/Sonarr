using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Migrator.Framework;
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
    public class RepositoryProviderTest
    {
        [Test]
        public void Get_Assembly_repos()
        {
            var provider = new RepositoryProvider();
            var types = provider.GetRepositoryTypes();

            Assert.IsNotEmpty(types);
            Assert.Contains(types, typeof(Config));
            Assert.Contains(types, typeof(Episode));
            Assert.Contains(types, typeof(EpisodeFile));
            Assert.Contains(types, typeof(ExternalNotificationSetting));
            Assert.Contains(types, typeof(History));
            Assert.Contains(types, typeof(IndexerSetting));
            Assert.Contains(types, typeof(JobSetting));
            Assert.Contains(types, typeof(RootDir));
            Assert.Contains(types, typeof(Season));
            Assert.Contains(types, typeof(Series));

            Assert.Contains(types, typeof(QualityProfile));

            Assert.DoesNotContain(types, typeof(QualityTypes));
        }





        [Test]
        public void Get_table_columns()
        {
            var provider = new RepositoryProvider();
            var typeTable = provider.GetSchemaFromType(typeof(TestRepoType));

            Assert.IsNotNull(typeTable.Columns);
            Assert.Count(3, typeTable.Columns);
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

            repo.Add(new TestRepoType(){Value = "Dummy"});

            var repositoryProvider = new RepositoryProvider();
            var columns = repositoryProvider.GetColumnsFromDatabase(connectionString, "TestRepoTypes");

            Assert.Count(3, columns);

        }


        [Test]
        public void DeleteColumns()
        {
            string connectionString = "Data Source=" + Guid.NewGuid() + ".db;Version=3;New=True";
            var dbProvider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");
            var repo = new SimpleRepository(dbProvider, SimpleRepositoryOptions.RunMigrations);

            repo.Add(new TestRepoType(){Value = "Dummy"});

            var repositoryProvider = new RepositoryProvider();
            var typeSchema = repositoryProvider.GetSchemaFromType(typeof(TestRepoType2));
            var columns = repositoryProvider.GetColumnsFromDatabase(connectionString, "TestRepoTypes");


            var deletedColumns = repositoryProvider.GetDeletedColumns(typeSchema, columns);


            Assert.Count(1, deletedColumns);
            Assert.AreEqual("NewName", deletedColumns[0].Name.Trim('[', ']'));
        }


        [Test]
        public void NewColumns()
        {
            string connectionString = "Data Source=" + Guid.NewGuid() + ".db;Version=3;New=True";
            var dbProvider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");
            var repo = new SimpleRepository(dbProvider, SimpleRepositoryOptions.RunMigrations);

            repo.Add(new TestRepoType2() { Value = "dummy" });

            var repositoryProvider = new RepositoryProvider();
            var typeSchema = repositoryProvider.GetSchemaFromType(typeof(TestRepoType));
            var columns = repositoryProvider.GetColumnsFromDatabase(connectionString, "TestRepoType2s");
            

            var deletedColumns = repositoryProvider.GetNewColumns(typeSchema, columns);


            Assert.Count(1, deletedColumns);
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
