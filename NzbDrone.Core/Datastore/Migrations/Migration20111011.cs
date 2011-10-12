using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20111011)]
    public class Migration20111011 : Migration
    {
        public override void Up()
        {
            Database.AddColumn("Episodes", "PostDownloadStatus", DbType.Int32, ColumnProperty.Null);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}