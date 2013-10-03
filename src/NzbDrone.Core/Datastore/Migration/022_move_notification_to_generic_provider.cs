using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(22)]
    public class move_indexer_to_generic_provider : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Indexers").AddColumn("ConfigContract").AsString().Nullable();

            //Execute.WithConnection(ConvertSeasons);
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
                            seasonsCmd.CommandText = String.Format(@"SELECT SeasonNumber, Monitored FROM Seasons WHERE SeriesId = {0}", seriesId);

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
                            var text = String.Format("UPDATE Series SET Seasons = '{0}' WHERE Id = {1}", seasons.ToJson(), seriesId);

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
