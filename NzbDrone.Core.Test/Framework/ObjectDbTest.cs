using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Test.Framework
{
    public abstract class ObjectDbTest<TSubject, TModel> : ObjectDbTest
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

    public abstract class ObjectDbTest : CoreTest
    {

        private IObjectDatabase _db;
        protected IObjectDatabase Db
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
            _db = new SiaqoDbFactory(new DiskProvider(), new EnvironmentProvider()).Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()));
            Mocker.SetConstant(Db);
        }

        [SetUp]
        public void SetupReadDb()
        {
            WithObjectDb();
        }

        [TearDown]
        public void ObjectDbTearDown()
        {
            if (_db != null)
            {
                _db.Dispose();
            }
        }
    }
}