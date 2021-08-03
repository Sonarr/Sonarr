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
            using (IDbCommand allSeriesCmd = conn.CreateCommand())
            {
                allSeriesCmd.Transaction = tran;
                allSeriesCmd.CommandText = @"SELECT Id FROM Series";
                using (IDataReader allSeriesReader = allSeriesCmd.ExecuteReader())
                {
                    while (allSeriesReader.Read())
                    {
                        int seriesId = allSeriesReader.GetInt32(0);
                        var seasons = new List<dynamic>();

                        using (IDbCommand seasonsCmd = conn.CreateCommand())
                        {
                            seasonsCmd.Transaction = tran;
                            seasonsCmd.CommandText = string.Format(@"SELECT SeasonNumber, Monitored FROM Seasons WHERE SeriesId = {0}", seriesId);

                            using (IDataReader seasonReader = seasonsCmd.ExecuteReader())
                            {
                                while (seasonReader.Read())
                                {
                                    int seasonNumber = seasonReader.GetInt32(0);
                                    bool monitored = seasonReader.GetBoolean(1);

                                    if (seasonNumber == 0)
                                    {
                                        monitored = false;
                                    }

                                    seasons.Add(new { seasonNumber, monitored });
                                }
                            }
                        }

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            var text = string.Format("UPDATE Series SET Seasons = '{0}' WHERE Id = {1}", seasons.ToJson(), seriesId);

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
