using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20120802)]
    public class Migration20120802 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("EpisodeFiles", new Column("SceneName", DbType.String, ColumnProperty.Null));
        }
    }
}