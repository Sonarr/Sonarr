using System.Data;
using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;
using Workarr.Tv;

namespace Workarr.Datastore.Migrations
{
    [Migration(166)]
    public class update_series_sort_title : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(UpdateSortTitles);
        }

        private void UpdateSortTitles(IDbConnection conn, IDbTransaction tran)
        {
            using (var getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = "SELECT \"Id\", \"TvdbId\", \"Title\" FROM \"Series\"";
                using (var seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var id = seriesReader.GetInt32(0);
                        var tvdbId = seriesReader.GetInt32(1);
                        var title = seriesReader.GetString(2);

                        var sortTitle = SeriesTitleNormalizer.Normalize(title, tvdbId);

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE \"Series\" SET \"SortTitle\" = ? WHERE \"Id\" = ?";
                            updateCmd.AddParameter(sortTitle);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
