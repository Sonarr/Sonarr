using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentMigrator.Runner;
using Marr.Data;
using NUnit.Framework;
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
                    throw new InvalidOperationException("Test object database doesn't exists. Make sure you call WithRealDb() if you intend to use an actual database.");

                return _db;
            }
        }

        protected virtual ITestDatabase WithTestDb(MigrationContext migrationContext)
        {
            var factory = Mocker.Resolve<DbFactory>();
            var database = factory.Create(migrationContext);
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

        protected void SetupContainer()
        {
            WithTempAsAppPath();

            Mocker.SetConstant<IConnectionStringFactory>(Mocker.Resolve<ConnectionStringFactory>());
            Mocker.SetConstant<IMigrationController>(Mocker.Resolve<MigrationController>());

            MapRepository.Instance.EnableTraceLogging = true;
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
            if (TestFolderInfo != null && Directory.Exists(TestFolderInfo.AppDataFolder))
            {
                var files = Directory.GetFiles(TestFolderInfo.AppDataFolder);

                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
    }
}