using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Services.Service.Migrations
{

    [Migration(20120203)]
    public class Migration20120203 : Migration
    {
        public override void Up()
        {
            Database.AddTable("ExceptionReports", new[]
                                            {
                                                 new Column("Type", DbType.String, ColumnProperty.PrimaryKey),
                                                 new Column("Logger", DbType.String, ColumnProperty.PrimaryKey),
                                                 new Column("LogMessage", DbType.String, ColumnProperty.PrimaryKey),
                                                 new Column("String", DbType.String, 4000, ColumnProperty.PrimaryKey),
                                                 MigrationsHelper.UGuidColumn,
                                                 MigrationsHelper.TimestampColumn,
                                                 MigrationsHelper.VersionColumn,
                                                 MigrationsHelper.ProductionColumn
                                             });
        }
        
        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}