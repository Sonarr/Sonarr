using System;
using System.Data;
using System.Linq;
using Migrator.Framework;

namespace NzbDrone.Services.Service.Migrations
{
    [Migration(20120226)]
    public class Migration20120226 : Migration
    {
        public override void Up()
        {
            Database.RenameTable("PendingSceneMappings", "OldPendingSceneMappings");

            Database.AddTable("PendingSceneMappings", new[]
                                            {
                                                 new Column("MappingId", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity), 
                                                 new Column("CleanTitle", DbType.String, ColumnProperty.NotNull),
                                                 new Column("Id", DbType.Int32, ColumnProperty.NotNull),
                                                 new Column("Title", DbType.String, ColumnProperty.NotNull)
                                            });

            Database.ExecuteNonQuery(@"INSERT INTO PendingSceneMappings (CleanTitle, Id, Title)
                                            SELECT CleanTitle, Id, Title
                                            FROM OldPendingSceneMappings");

            Database.RemoveTable("OldPendingSceneMappings");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}