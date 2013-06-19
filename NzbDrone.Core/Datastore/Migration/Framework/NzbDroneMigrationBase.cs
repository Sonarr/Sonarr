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
            switch ((MigrationType)ApplicationContext)
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
