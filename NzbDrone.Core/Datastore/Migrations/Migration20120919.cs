using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20120919)]
    public class Migration20120919 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("Series", new Column("CustomStartDate", DbType.DateTime, ColumnProperty.Null));

            Database.ExecuteNonQuery("UPDATE Series SET CustomStartDate = DownloadEpisodesAiredAfter");

            Database.RemoveColumn("Series", "DownloadEpisodesAiredAfter");
        }
    }
}