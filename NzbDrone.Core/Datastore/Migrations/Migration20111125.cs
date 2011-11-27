using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20111125)]
    public class Migration2011125 : Migration
    {
        public override void Up()
        {
            Database.AddColumn("Series", "IsDaily", DbType.Boolean, ColumnProperty.Null);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}