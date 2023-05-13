using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(20)]
    public class add_year_and_seasons_to_series : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("Year").AsInt32().Nullable();
            Alter.Table("Series").AddColumn("Seasons").AsString().Nullable();

            Execute.WithConnection(ConvertSeasons);
        }

        private void ConvertSeasons(IDbConnection conn, IDbTransaction tran)
        {
            using (var allSeriesCmd = conn.CreateCommand())
            {
                allSeriesCmd.Transaction = tran;
                allSeriesCmd.CommandText = "SELECT \"Id\" FROM \"Series\"";
                using (var allSeriesReader = allSeriesCmd.ExecuteReader())
                {
                    while (allSeriesReader.Read())
                    {
                        var seriesId = allSeriesReader.GetInt32(0);
                        var seasons = new List<dynamic>();

                        using (var seasonsCmd = conn.CreateCommand())
                        {
                            seasonsCmd.Transaction = tran;
                            seasonsCmd.CommandText = $"SELECT \"SeasonNumber\", \"Monitored\" FROM \"Seasons\" WHERE \"SeriesId\" = {seriesId}";

                            using (var seasonReader = seasonsCmd.ExecuteReader())
                            {
                                while (seasonReader.Read())
                                {
                                    var seasonNumber = seasonReader.GetInt32(0);
                                    var monitored = seasonReader.GetBoolean(1);

                                    if (seasonNumber == 0)
                                    {
                                        monitored = false;
                                    }

                                    seasons.Add(new { seasonNumber, monitored });
                                }
                            }
                        }

                        using (var updateCmd = conn.CreateCommand())
                        {
                            var text = $"UPDATE \"Series\" SET \"Seasons\" = '{seasons.ToJson()}' WHERE \"Id\" = {seriesId}";

                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = text;
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
