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
            if ((MigrationType)ApplicationContext == MigrationType.Main)
            {
                MainDbUpgrade();
            }
            else if ((MigrationType)ApplicationContext == MigrationType.Log)
            {
                LogDbUpgrade();
            }
            else
            {
                LogDbUpgrade();
                MainDbUpgrade();
            }
        }


        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
