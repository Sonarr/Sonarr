using System;
using System.Data;
using System.Linq;
using Migrator.Framework;

namespace NzbDrone.Services.Service.Migrations
{
    [Migration(20121223)]
    public class Migration20121223 : Migration
    {
        public override void Up()
        {
            Database.AddColumn("SceneMappings", new Column("Season", DbType.Int32, ColumnProperty.Null));
            Database.ExecuteNonQuery("UPDATE SceneMappings SET Season = -1 WHERE Season IS NULL");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}