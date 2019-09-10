using System;
using FluentMigrator;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public abstract class NzbDroneMigrationBase : FluentMigrator.Migration
    {
        protected readonly Logger _logger;
        private MigrationContext _migrationContext;

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
                    _logger.Info("Starting migration to " + Version);
                    MainDbUpgrade();
                    return;
                case MigrationType.Log:
                    _logger.Info("Starting migration to " + Version);
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
