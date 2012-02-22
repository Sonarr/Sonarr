using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20120220)]
    public class Migration20120220 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddTable("Seasons", new[]
                                            {
                                                new Column("SeasonId", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                new Column("SeriesId", DbType.Int32, ColumnProperty.NotNull), 
                                                new Column("SeasonNumber", DbType.Int32, ColumnProperty.NotNull), 
                                                new Column("Ignored", DbType.Boolean, ColumnProperty.NotNull)
                                            });

            Database.ExecuteNonQuery(@"INSERT INTO Seasons (SeriesId, SeasonNumber, Ignored)
                                            SELECT SeriesId, SeasonNumber,
                                            CASE WHEN Count(*) = 
                                            SUM(CASE WHEN Ignored = 1 THEN 1 ELSE 0 END) THEN 1 ELSE 0 END AS Ignored
                                            FROM Episodes
                                            GROUP BY SeriesId, SeasonNumber");
        }
    }
}