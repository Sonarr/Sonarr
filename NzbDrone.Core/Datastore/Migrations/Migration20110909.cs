using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20110909)]
    public class Migration20110909 : Migration
    {
        public override void Up()
        {
            Database.AddColumn("Series", "Runtime", DbType.Int32, ColumnProperty.Null);
            Database.AddColumn("Series", "BannerUrl", DbType.String, ColumnProperty.Null);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}