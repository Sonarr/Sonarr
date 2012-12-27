using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20121226)]
    public class Migration20121226 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("Series", new Column("FirstAired", DbType.DateTime, ColumnProperty.Null));
        }
    }
}