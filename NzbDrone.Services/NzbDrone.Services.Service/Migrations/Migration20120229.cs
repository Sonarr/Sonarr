using System;
using System.Data;
using System.Linq;
using Migrator.Framework;

namespace NzbDrone.Services.Service.Migrations
{
    [Migration(20120229)]
    public class Migration20120229 : Migration
    {
        public override void Up()
        {
 
            Database.AddTable("ExceptionInstances", new Column("Id", DbType.Int64, ColumnProperty.PrimaryKeyWithIdentity),
                                                    new Column("ExceptionDetail", DbType.Int16, ColumnProperty.NotNull),
                                                    new Column("LogMessage", DbType.String, 3000, ColumnProperty.NotNull),
                                                    MigrationsHelper.TimestampColumn,
                                                    MigrationsHelper.ProductionColumn);

            Database.AddTable("Exceptions", new Column("Id", DbType.Int64, ColumnProperty.PrimaryKeyWithIdentity),
                                                   new Column("Logger", DbType.String, ColumnProperty.NotNull),
                                                   new Column("Type", DbType.String, ColumnProperty.NotNull),
                                                   new Column("String", DbType.String, ColumnProperty.NotNull),
                                                   new Column("Hash", DbType.String, ColumnProperty.NotNull),
                                                   MigrationsHelper.VersionColumn);


            Database.ExecuteNonQuery("ALTER TABLE ExceptionReports ALTER COLUMN String NTEXT");
            Database.ExecuteNonQuery("ALTER TABLE Exceptions ALTER COLUMN String NTEXT");

        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}