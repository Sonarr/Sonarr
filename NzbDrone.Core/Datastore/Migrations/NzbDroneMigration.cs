using System;
using System.Data.SqlServerCe;
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

        protected EloqueraDb GetObjectDb()
        {

            var sqlCeConnection = new SqlCeConnection(Database.ConnectionString);

            var eqPath = sqlCeConnection.Database.Replace(".sdf", ".eq");
            return new EloqueraDbFactory(new EnvironmentProvider()).Create(eqPath);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
