using System;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public abstract class NzbDroneMigrationBase : FluentMigrator.Migration
    {
        protected virtual void MainDbUpgrade()
        {
        }

        protected virtual void LogDbUpgrade()
        {
        }

        public override void Up()
        {
            var context = (MigrationContext)ApplicationContext;

            SQLiteAlter = context.SQLiteAlter;

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

        protected ISQLiteAlter SQLiteAlter { get; private set; }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
