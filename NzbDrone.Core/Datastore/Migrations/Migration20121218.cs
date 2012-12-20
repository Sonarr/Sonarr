using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20121218)]
    public class Migration20121218 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("Series", new Column("TvRageId", DbType.Int32, ColumnProperty.Null));
            Database.AddColumn("Series", new Column("TvRageTitle", DbType.String, ColumnProperty.Null));
            Database.AddColumn("Series", new Column("UtcOffset", DbType.Int32, ColumnProperty.Null));
        }
    }
}