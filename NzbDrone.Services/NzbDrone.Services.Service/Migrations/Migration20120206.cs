using System;
using Migrator.Framework;

namespace NzbDrone.Services.Service.Migrations
{

    [Migration(20120206)]
    public class Migration20120206 : Migration
    {
        public override void Up()
        {
            Database.ExecuteNonQuery("ALTER TABLE ExceptionReports DROP CONSTRAINT PK_ExceptionReports");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}