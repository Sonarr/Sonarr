using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Migration.Framework;

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
            var factory = Mocker.Resolve<DbFactory>();

            // If a special migration test or log migration then create new
            if (migrationContext.BeforeMigration != null)
            {
                return factory.Create(migrationContext);
            }

            // Otherwise try to use a cached migrated db
            var cachedDb = GetCachedDatabase(migrationContext.MigrationType);
            var testDb = GetTestDb(migrationContext.MigrationType);
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

        private string GetCachedDatabase(MigrationType type)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, $"cached_{type}.db");
        }

        private string GetTestDb(MigrationType type)
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

            if (TestFolderInfo != null)
            {
                DeleteTempFolder(TestFolderInfo.AppDataFolder);
            }
        }
    }
}
