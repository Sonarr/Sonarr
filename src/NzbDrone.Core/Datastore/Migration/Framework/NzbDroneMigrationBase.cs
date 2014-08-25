using System;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public abstract class NzbDroneMigrationBase : FluentMigrator.Migration
    {
        protected readonly Logger _logger;

        protected NzbDroneMigrationBase()
        {
            _logger = NzbDroneLogger.GetLogger(this);
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

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
