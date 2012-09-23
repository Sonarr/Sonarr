using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20120918)]
    public class Migration20120918 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("Series", new Column("DownloadEpisodesAiredAfter", DbType.DateTime, ColumnProperty.Null));
        }
    }
}