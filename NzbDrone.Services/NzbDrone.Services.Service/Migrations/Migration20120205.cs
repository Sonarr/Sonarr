using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Services.Service.Migrations
{

    [Migration(20120205)]
    public class Migration20120205 : Migration
    {
        public override void Up()
        {
            Database.ChangeColumn("ParseErrorReports", MigrationsHelper.VersionColumn);
            Database.ChangeColumn("ExceptionReports", MigrationsHelper.VersionColumn);
        }
        
        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}