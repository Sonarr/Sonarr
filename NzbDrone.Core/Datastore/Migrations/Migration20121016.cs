using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20121016)]
    public class Migration20121016 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("Episodes", new Column("SceneAbsoluteEpisodeNumber", DbType.Int32, ColumnProperty.Null));
            Database.AddColumn("Episodes", new Column("SceneSeasonNumber", DbType.Int32, ColumnProperty.Null));
            Database.AddColumn("Episodes", new Column("SceneEpisodeNumber", DbType.Int32, ColumnProperty.Null));
        }
    }
}