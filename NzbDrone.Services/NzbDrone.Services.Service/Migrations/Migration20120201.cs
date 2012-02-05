using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Services.Service.Migrations
{

    [Migration(20120201)]
    public class Migration20120201 : Migration
    {
        public override void Up()
        {

            Database.AddTable("SceneMappings", new[]
                                            {
                                                 new Column("CleanTitle", DbType.String, ColumnProperty.PrimaryKey),
                                                 new Column("Id", DbType.Int32, ColumnProperty.NotNull),
                                                 new Column("Title", DbType.String, ColumnProperty.NotNull)
                                            });

            Database.AddTable("PendingSceneMappings", new[]
                                            {
                                                 new Column("CleanTitle", DbType.String, ColumnProperty.PrimaryKey),
                                                 new Column("Id", DbType.Int32, ColumnProperty.NotNull),
                                                 new Column("Title", DbType.String, ColumnProperty.NotNull)
                                            });

            Database.AddTable("DailySeries", new[]
                                            {
                                                 new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                                                 new Column("Title", DbType.String, ColumnProperty.NotNull)
                                            });

            Database.AddTable("ParseErrorReports", new[]
                                            {
                                                 new Column("Title", DbType.String,1000, ColumnProperty.PrimaryKey),
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