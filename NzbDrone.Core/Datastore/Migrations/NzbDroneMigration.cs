using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{
    public abstract class NzbDroneMigration : Migration
    {
        protected virtual void MainDbUpgrade()
        {
        }

        protected virtual void LogDbUpgrade()
        {
        }

        public override void Up()
        {
            if ((MigrationType)this.ApplicationContext == MigrationType.Main)
            {
                MainDbUpgrade();
            }
            else if ((MigrationType)this.ApplicationContext == MigrationType.Log)
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
