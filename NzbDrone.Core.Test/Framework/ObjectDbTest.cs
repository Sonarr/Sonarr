using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Test.Framework
{

    public abstract class ObjectDbTest<TSubject> : ObjectDbTest where TSubject : class
    {
        private TSubject _subject;

        [SetUp]
        public void CoreTestSetup()
        {
            _subject = null;
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

        protected void WithObjectDb(bool memory = true)
        {
            if (memory)
            {
                _db = new SiaqoDbFactory(new DiskProvider()).CreateMemoryDb();
            }
            else
            {
                _db = new SiaqoDbFactory(new DiskProvider()).Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()));
            }

            Mocker.SetConstant(Db);
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