using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20121012)]
    public class Migration20121012 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("Episodes", new Column("AbsoluteEpisodeNumber", DbType.Int32, ColumnProperty.Null));
        }
    }
}