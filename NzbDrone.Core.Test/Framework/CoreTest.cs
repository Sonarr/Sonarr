using System;
using System.IO;
using NUnit.Framework;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Test.Common;
using PetaPoco;

namespace NzbDrone.Core.Test.Framework
{
    public class CoreTest : TestBase
    // ReSharper disable InconsistentNaming
    {
        static CoreTest()
        {
            //Delete old db files
            var oldDbFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sdf", SearchOption.AllDirectories);
            foreach (var file in oldDbFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            //Delete App_data folder
            var appData = new EnviromentProvider().GetAppDataPath();

            if (Directory.Exists(appData))
            {
                Directory.Delete(appData, true);
            }

            TestDbHelper.CreateDataBaseTemplate();
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
            private set { _db = value; }
        }

        protected void WithRealDb()
        {
            Db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(Db);
        }


        protected ProgressNotification MockNotification
        {
            get
            {
                return new ProgressNotification("Mock notification");
            }
        }
    }
}
