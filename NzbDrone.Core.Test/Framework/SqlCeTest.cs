using System;
using System.IO;
using Db4objects.Db4o.IO;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Test.Common;
using PetaPoco;

namespace NzbDrone.Core.Test.Framework
{

    public abstract class CoreTest : TestBase
    {
        protected static ProgressNotification MockNotification
        {
            get
            {
                return new ProgressNotification("Mock notification");
            }
        }

        protected static void ThrowException()
        {
            throw new ApplicationException("This is a message for test exception");
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
                //Todo: Actually use memory: http://www.eloquera.com/sites/default/files/filepicker/1/Help/Documentation/HTML/In-memory%20database.htm
                _db = new EloqueraDbFactory().Create();
            }
            else
            {
                _db = new EloqueraDbFactory().Create(dbFilename: Guid.NewGuid().ToString());
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


    public abstract class SqlCeTest : CoreTest
    {
        private string _dbTemplateName;

        [SetUp]
        public void CoreTestSetup()
        {
            if (NCrunch.Framework.NCrunchEnvironment.NCrunchIsResident())
            {
                _dbTemplateName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName()) + ".sdf";
            }
            else
            {
                _dbTemplateName = "db_template.sdf";
            }

            CreateDataBaseTemplate();
        }

        private IDatabase GetEmptyDatabase(string fileName = "")
        {
            Console.WriteLine("====================DataBase====================");
            Console.WriteLine("Cloning database from template.");

            if (String.IsNullOrWhiteSpace(fileName))
            {
                fileName = Guid.NewGuid() + ".sdf";
            }

            File.Copy(_dbTemplateName, fileName);

            var connectionString = ConnectionFactory.GetConnectionString(fileName);
            var database = ConnectionFactory.GetPetaPocoDb(connectionString);

            Console.WriteLine("====================DataBase====================");
            Console.WriteLine();
            Console.WriteLine();

            return database;
        }

        private void CreateDataBaseTemplate()
        {
            Console.WriteLine("Creating an empty PetaPoco database");
            var connectionString = ConnectionFactory.GetConnectionString(_dbTemplateName);
            var database = ConnectionFactory.GetPetaPocoDb(connectionString);
            database.Dispose();
        }



        private IDatabase _db;
        protected IDatabase Db
        {
            get
            {
                if (_db == null)
                    throw new InvalidOperationException("Test db doesn't exists. Make sure you call WithRealDb() if you intend to use an actual database.");

                return _db;
            }
        }



        protected void WithRealDb()
        {
            _db = GetEmptyDatabase();
            Mocker.SetConstant(Db);
        }


        [TearDown]
        public void CoreTestTearDown()
        {
            ConfigProvider.ClearCache();

            if (_db != null && _db.Connection != null && File.Exists(_db.Connection.Database))
            {
                var file = _db.Connection.Database;
                _db.Dispose();
                try
                {
                    File.Delete(file);

                }
                catch (IOException) { }
            }
        }
    }
}
