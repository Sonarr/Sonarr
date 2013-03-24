using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using ServiceStack.OrmLite;

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

        private IDatabase _db;
        protected IDatabase Db
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
            var factory = new DbFactory();
            var dbConnection = factory.Create();
            _db = new TestDatabase(dbConnection);
            Mocker.SetConstant(dbConnection);
        }

        [SetUp]
        public void SetupReadDb()
        {
            WithObjectDb();
        }


    }


    public interface IDatabase
    {
        void InsertMany<T>(IEnumerable<T> items) where T : new();
        void Insert<T>(T item) where T : new();
        IEnumerable<T> All<T>() where T : new();
        void Update<T>(T childModel) where T : new();
        void Delete<T>(T childModel) where T : new();
    }

    public class TestDatabase : IDatabase
    {
        private readonly IDbConnection _dbConnection;

        public TestDatabase(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public void InsertMany<T>(IEnumerable<T> items) where T : new()
        {
            _dbConnection.InsertAll(items);
        }

        public void Insert<T>(T item) where T : new()
        {
            _dbConnection.Insert(item);
        }

        public IEnumerable<T> All<T>() where T : new()
        {
            return _dbConnection.Select<T>();
        }

        public void Update<T>(T childModel) where T : new()
        {
            _dbConnection.Update(childModel);
        }

        public void Delete<T>(T childModel) where T : new()
        {
            _dbConnection.Delete(childModel);
        }
    }
}