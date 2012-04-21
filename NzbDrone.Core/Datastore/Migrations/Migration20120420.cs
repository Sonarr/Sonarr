using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20120420)]
    public class Migration20120420 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddTable("SearchResults", new[]
                                            {
                                                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                new Column("SeriesId", DbType.Int32, ColumnProperty.NotNull),
                                                new Column("SeasonNumber", DbType.Int32, ColumnProperty.Null),
                                                new Column("EpisodeId", DbType.Int32, ColumnProperty.Null),
                                                new Column("SearchTime", DbType.DateTime, ColumnProperty.NotNull),
                                                new Column("SuccessfulDownload", DbType.Boolean, ColumnProperty.NotNull)
                                            });

            Database.AddTable("SearchResultItems", new[]
                                            {
                                                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                new Column("SearchResultId", DbType.Int32, ColumnProperty.NotNull),
                                                new Column("ReportTitle", DbType.String, ColumnProperty.NotNull),
                                                new Column("Indexer", DbType.String, ColumnProperty.NotNull),
                                                new Column("NzbUrl", DbType.String, ColumnProperty.NotNull),
                                                new Column("NzbInfoUrl", DbType.String, ColumnProperty.Null),
                                                new Column("Success", DbType.Boolean, ColumnProperty.NotNull),
                                                new Column("SearchError", DbType.Int32, ColumnProperty.NotNull)
                                            });
        }
    }
}