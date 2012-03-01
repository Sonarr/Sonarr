using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Migrator.Providers.SqlServer;
using NUnit.Framework;
using NzbDrone.Services.Service.Migrations;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;
using Services.PetaPoco;


namespace NzbDrone.Services.Tests.Framework
{
    public abstract class ServicesTestBase : LoggingTest
    {
        static readonly string connectionString = ConfigurationManager.ConnectionStrings["SqlExpress"].ConnectionString;

        static ServicesTestBase()
        {

        }

        private static void ResetDb()
        {
            var transformationProvider = new SqlServerTransformationProvider(new SqlServerDialect(), connectionString);

            var tables = transformationProvider.GetTables();

            foreach (var table in tables)
            {
                transformationProvider.RemoveTable(table);
            }


            MigrationsHelper.Run(connectionString);
        }

        public IDatabase Db { get; private set; }

        private AutoMoqer _mocker;
        protected AutoMoqer Mocker
        {
            get
            {
                if (_mocker == null)
                {
                    _mocker = new AutoMoqer();
                }

                return _mocker;
            }
        }

        protected void WithRealDb()
        {
            ResetDb();

            Db = new Database("SqlExpress");
            Mocker.SetConstant(Db);
        }

        [TearDown]
        public void ServiceTearDown()
        {
            ExceptionVerification.IgnoreWarns();
        }

    }
}
