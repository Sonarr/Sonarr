using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Common;
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

        protected IList<TModel> AllStoredModels
        {
            get
            {
                return Storage.All().ToList();
            }
        }

        protected TModel StoredModel
        {
            get
            {
                return Storage.All().Single();
            }
        }

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




    public abstract class DbTest : CoreTest
    {

        private string _dbName;

        private ITestDatabase _db;
        private IDatabase _database;

        protected ITestDatabase Db
        {
            get
            {
                if (_db == null)
                    throw new InvalidOperationException("Test object database doesn't exists. Make sure you call WithRealDb() if you intend to use an actual database.");

                return _db;
            }
        }

        private void WithObjectDb(bool memory = true)
        {

            _dbName = DateTime.Now.Ticks.ToString() + ".db";

            MapRepository.Instance.EnableTraceLogging = true;

            var factory = new DbFactory(new MigrationController(new NlogAnnouncer()));
            _database = factory.Create(_dbName);
            _db = new TestTestDatabase(_database);
            Mocker.SetConstant(_database);
        }

        [SetUp]
        public void SetupReadDb()
        {
            WithObjectDb();
        }

        [TearDown]
        public void TearDown()
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.db");

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


    public interface ITestDatabase
    {
        void InsertMany<T>(IEnumerable<T> items) where T : ModelBase, new();
        void Insert<T>(T item) where T : ModelBase, new();
        IEnumerable<T> All<T>() where T : ModelBase, new();
        T Single<T>() where T : ModelBase, new();
        void Update<T>(T childModel) where T : ModelBase, new();
        void Delete<T>(T childModel) where T : ModelBase, new();
    }

    public class TestTestDatabase : ITestDatabase
    {
        private readonly IDatabase _dbConnection;

        public TestTestDatabase(IDatabase dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public void InsertMany<T>(IEnumerable<T> items) where T : ModelBase, new()
        {
            new BasicRepository<T>(_dbConnection).InsertMany(items.ToList());
        }

        public void Insert<T>(T item) where T : ModelBase, new()
        {
            new BasicRepository<T>(_dbConnection).Insert(item);
        }

        public IEnumerable<T> All<T>() where T : ModelBase, new()
        {
            return new BasicRepository<T>(_dbConnection).All();
        }

        public T Single<T>() where T : ModelBase, new()
        {
            return All<T>().SingleOrDefault();
        }

        public void Update<T>(T childModel) where T : ModelBase, new()
        {
            new BasicRepository<T>(_dbConnection).Update(childModel);
        }

        public void Delete<T>(T childModel) where T : ModelBase, new()
        {
            new BasicRepository<T>(_dbConnection).Delete(childModel);
        }
    }
}