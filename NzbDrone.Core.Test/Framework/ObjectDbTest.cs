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

        private EloqueraDb _db;
        protected EloqueraDb Db
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
                _db = new EloqueraDbFactory(new EnvironmentProvider()).CreateMemoryDb();
            }
            else
            {
                _db = new EloqueraDbFactory(new EnvironmentProvider()).Create(Path.Combine(Environment.CurrentDirectory,Guid.NewGuid().ToString()+ ".elq");
            }

            Mocker.SetConstant(Db);
            Mocker.SetConstant(Db.Db);
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