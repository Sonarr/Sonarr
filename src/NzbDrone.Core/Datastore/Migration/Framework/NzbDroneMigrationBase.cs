using System;
using FluentMigrator;
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

        public int Version
        {
            get
            {
                var migrationAttribute = (MigrationAttribute)Attribute.GetCustomAttribute(GetType(), typeof(MigrationAttribute));
                return (int)migrationAttribute.Version;
            }
        }

        public override void Up()
        {
            if (MigrationContext.Current.BeforeMigration != null)
            {
                MigrationContext.Current.BeforeMigration(this);
            }

            switch (MigrationContext.Current.MigrationType)
            {
                case MigrationType.Main:
                    LogMigrationMessage(MigrationType.Main);
                    MainDbUpgrade();
                    return;
                case MigrationType.Log:
                    LogMigrationMessage(MigrationType.Log);
                    LogDbUpgrade();
                    return;
                default:
                    LogMigrationMessage(MigrationType.Log);
                    LogDbUpgrade();

                    LogMigrationMessage(MigrationType.Main);
                    MainDbUpgrade();
                    return;
            }
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        private void LogMigrationMessage(MigrationType type)
        {
            _logger.Info("Starting migration of {0} DB to {1}", type.ToString(), Version);
        }
    }
}
