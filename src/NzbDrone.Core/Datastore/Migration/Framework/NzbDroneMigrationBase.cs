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

        public MigrationContext Context
        {
            get
            {
                if (_migrationContext == null)
                {
                    _migrationContext = (MigrationContext)ApplicationContext;
                }
                return _migrationContext;
            }
        }

        public override void Up()
        {
            if (Context.BeforeMigration != null)
            {
                Context.BeforeMigration(this);
            }

            switch (Context.MigrationType)
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
