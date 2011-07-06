using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20110705)]
    public class Migration20110705 : Migration
    {
        public override void Up()
        {
            //Upgrade column size
            Database.ChangeColumn("Series", new Column("Overview", DbType.String, 4000, ColumnProperty.Null));
            Database.ChangeColumn("Series", new Column("Path", DbType.String, 4000, ColumnProperty.NotNull));

            Database.ChangeColumn("Episodes", new Column("Overview", DbType.String, 4000, ColumnProperty.Null));

            Database.ChangeColumn("EpisodeFiles", new Column("Path", DbType.String, 4000, ColumnProperty.NotNull));

            Database.ChangeColumn("RootDirs", new Column("Path", DbType.String, 4000, ColumnProperty.NotNull));

            Database.ChangeColumn("Logs", new Column("Message", DbType.String, 4000, ColumnProperty.NotNull));
            Database.ChangeColumn("Logs", new Column("Exception", DbType.String, 4000, ColumnProperty.Null));
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}