using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
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
            if (Database.ConnectionString.Contains(PathExtentions.NZBDRONE_SQLCE_DB_FILE))
            {
                MainDbUpgrade();
            }
            else if (Database.ConnectionString.Contains(PathExtentions.LOG_SQLCE_DB_FILE))
            {
                LogDbUpgrade();
            }
            else
            {
                LogDbUpgrade();
                MainDbUpgrade();
            }
        }

        protected IObjectDatabase GetObjectDb()
        {
            var sqlCeConnection = SqlCeProxy.EnsureDatabase(Database.ConnectionString);
            
            var eqPath = sqlCeConnection.Database.Replace(".sdf", ".eq");
            return new SiaqoDbFactory(new DiskProvider()).Create(eqPath);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
