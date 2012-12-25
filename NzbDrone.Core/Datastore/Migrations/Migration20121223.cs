using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20121223)]
    public class Migration20121223 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("SceneMappings", new Column("SeasonNumber", DbType.Int32, ColumnProperty.Null));
            Database.ExecuteNonQuery("UPDATE SceneMappings SET SeasonNumber = -1 WHERE SeasonNumber IS NULL");
        }
    }
}