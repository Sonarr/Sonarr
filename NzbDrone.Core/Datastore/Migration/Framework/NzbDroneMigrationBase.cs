using System;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public abstract class NzbDroneMigrationBase : FluentMigrator.Migration
    {
        private Logger _logger;

        protected NzbDroneMigrationBase()
        {
            _logger = NzbDroneLogger.GetLogger();
        }

        protected virtual void MainDbUpgrade()
        {
        }

        protected virtual void LogDbUpgrade()
        {
        }

        public override void Up()
        {
            var context = (MigrationContext)ApplicationContext;

            SqLiteAlter = context.SQLiteAlter;
            MigrationHelper = context.MigrationHelper;

            switch (context.MigrationType)
            {
                case MigrationType.Main:
                    MainDbUpgrade();
                    return;
                case MigrationType.Log:
                    LogDbUpgrade();
                    return;
                default:
                    LogDbUpgrade();
                    MainDbUpgrade();
                    return;
            }
        }

        protected ISQLiteAlter SqLiteAlter { get; private set; }
        protected ISqLiteMigrationHelper MigrationHelper { get; private set; }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
