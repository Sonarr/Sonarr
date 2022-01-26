using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Test.Common.Datastore;

namespace NzbDrone.Core.Test.Framework
{
    public abstract class DbTest<TSubject, TModel> : DbTest
        where TSubject : class
        where TModel : ModelBase, new()
    {
        private TSubject _subject;

        protected BasicRepository<TModel> Storage { get; private set; }

        protected IList<TModel> AllStoredModels => Storage.All().ToList();

        protected TModel StoredModel => Storage.All().Single();

        [SetUp]
        public void CoreTestSetup()
        {
            _subject = null;
            Storage = Mocker.Resolve<BasicRepository<TModel>>();
        }

        protected TSubject Subject
        {
            get
            {
                if (_subject == null)
                {
                    _subject = Mocker.Resolve<TSubject>();
                }

                return _subject;
            }
        }
    }

    [Category("DbTest")]
    public abstract class DbTest : CoreTest
    {
        private ITestDatabase _db;
        private DatabaseType _databaseType;

        protected virtual MigrationType MigrationType => MigrationType.Main;

        protected ITestDatabase Db
        {
            get
            {
                if (_db == null)
                {
                    throw new InvalidOperationException("Test object database doesn't exists. Make sure you call WithRealDb() if you intend to use an actual database.");
                }

                return _db;
            }
        }

        protected virtual ITestDatabase WithTestDb(MigrationContext migrationContext)
        {
            var database = CreateDatabase(migrationContext);
            Mocker.SetConstant(database);

            switch (MigrationType)
            {
                case MigrationType.Main:
                    {
                        var mainDb = new MainDatabase(database);

                        Mocker.SetConstant<IMainDatabase>(mainDb);
                        break;
                    }

                case MigrationType.Log:
                    {
                        var logDb = new LogDatabase(database);

                        Mocker.SetConstant<ILogDatabase>(logDb);
                        break;
                    }

                default:
                    {
                        throw new ArgumentException("Invalid MigrationType");
                    }
            }

            var testDb = new TestDatabase(database);

            return testDb;
        }

        private IDatabase CreateDatabase(MigrationContext migrationContext)
        {
            if (_databaseType == DatabaseType.PostgreSQL)
            {
                CreatePostgresDb();
            }

            var factory = Mocker.Resolve<DbFactory>();

            // If a special migration test or log migration then create new
            if (migrationContext.BeforeMigration != null || _databaseType == DatabaseType.PostgreSQL)
            {
                return factory.Create(migrationContext);
            }

            return CreateSqliteDatabase(factory, migrationContext);
        }

        private void CreatePostgresDb()
        {
            var options = Mocker.Resolve<IOptions<PostgresOptions>>().Value;
            PostgresDatabase.Create(options, MigrationType);
        }

        private void DropPostgresDb()
        {
            var options = Mocker.Resolve<IOptions<PostgresOptions>>().Value;
            PostgresDatabase.Drop(options, MigrationType);
        }

        private IDatabase CreateSqliteDatabase(IDbFactory factory, MigrationContext migrationContext)
        {
            // Otherwise try to use a cached migrated db
            var cachedDb = SqliteDatabase.GetCachedDb(migrationContext.MigrationType);
            var testDb = GetTestSqliteDb(migrationContext.MigrationType);
            if (File.Exists(cachedDb))
            {
                TestLogger.Info($"Using cached initial database {cachedDb}");
                File.Copy(cachedDb, testDb);
                return factory.Create(migrationContext);
            }
            else
            {
                var db = factory.Create(migrationContext);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                SQLiteConnection.ClearAllPools();

                TestLogger.Info("Caching database");
                File.Copy(testDb, cachedDb);
                return db;
            }
        }

        private string GetTestSqliteDb(MigrationType type)
        {
            return type == MigrationType.Main ? TestFolderInfo.GetDatabase() : TestFolderInfo.GetLogDatabase();
        }

        protected virtual void SetupLogging()
        {
            Mocker.SetConstant<ILoggerProvider>(NullLoggerProvider.Instance);
        }

        protected void SetupContainer()
        {
            WithTempAsAppPath();
            SetupLogging();

            // populate the possible postgres options
            var postgresOptions = PostgresDatabase.GetTestOptions();
            _databaseType = postgresOptions.Host.IsNotNullOrWhiteSpace() ? DatabaseType.PostgreSQL : DatabaseType.SQLite;

            // Set up remaining container services
            Mocker.SetConstant(Options.Create(postgresOptions));
            Mocker.SetConstant<IConfigFileProvider>(Mocker.Resolve<ConfigFileProvider>());
            Mocker.SetConstant<IConnectionStringFactory>(Mocker.Resolve<ConnectionStringFactory>());
            Mocker.SetConstant<IMigrationController>(Mocker.Resolve<MigrationController>());

            SqlBuilderExtensions.LogSql = true;
        }

        [SetUp]
        public virtual void SetupDb()
        {
            SetupContainer();
            _db = WithTestDb(new MigrationContext(MigrationType));
        }

        [TearDown]
        public void TearDown()
        {
            // Make sure there are no lingering connections. (When this happens it means we haven't disposed something properly)
            GC.Collect();
            GC.WaitForPendingFinalizers();

            SQLiteConnection.ClearAllPools();
            NpgsqlConnection.ClearAllPools();

            if (TestFolderInfo != null)
            {
                DeleteTempFolder(TestFolderInfo.AppDataFolder);
            }

            if (_databaseType == DatabaseType.PostgreSQL)
            {
                DropPostgresDb();
            }
        }
    }
}
