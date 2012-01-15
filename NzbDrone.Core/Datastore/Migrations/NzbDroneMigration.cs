using System;
using System.Linq;
using Migrator.Framework;
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
            if (Database.ConnectionString.Contains(PathExtentions.NZBDRONE_DB_FILE))
            {
                MainDbUpgrade();
            }
            else if (Database.ConnectionString.Contains(PathExtentions.LOG_DB_FILE))
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
