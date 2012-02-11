using System;
using System.Data;
using System.Linq;
using Migrator.Framework;

namespace NzbDrone.Services.Service.Migrations
{
    [Migration(20120210)]
    public class Migration20120210 : Migration
    {
        public override void Up()
        {
            Database.ChangeColumn("ExceptionReports", new Column("LogMessage", DbType.String, 4000, ColumnProperty.NotNull));
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}