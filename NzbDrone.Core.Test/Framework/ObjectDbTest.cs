using System;
using System.Linq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Test.Framework
{
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
                _db = new EloqueraDbFactory(new EnvironmentProvider()).Create();
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