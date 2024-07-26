using System.Collections.Generic;
using System.Data;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(210)]
    public class add_monitored_seasons_filter : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ChangeHasUnmonitoredSeason);
        }

        private void ChangeHasUnmonitoredSeason(IDbConnection conn, IDbTransaction tran)
        {
            var updated = new List<object>();
            using (var getUnmonitoredSeasonFilter = conn.CreateCommand())
            {
                getUnmonitoredSeasonFilter.Transaction = tran;
                getUnmonitoredSeasonFilter.CommandText = "SELECT \"Id\", \"Filters\" FROM \"CustomFilters\" WHERE \"Type\" = 'series'";

                using (var reader = getUnmonitoredSeasonFilter.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var filters = JArray.Parse(reader.GetString(1));

                        foreach (var filter in filters)
                        {
                            if (filter["key"]?.ToString() == "hasUnmonitoredSeason")
                            {
                                var value = filter["value"].ToString();
                                var type = filter["type"].ToString();

                                filter["key"] = "seasonsMonitoredStatus";

                                if (value.Contains("true") && value.Contains("false"))
                                {
                                    filter["value"] = new JArray("all", "partial", "none");
                                }
                                else if (value.Contains("true"))
                                {
                                    filter["type"] = type == "equal" ? "notEqual" : "equal";
                                    filter["value"] = new JArray("all");
                                }
                                else if (value.Contains("false"))
                                {
                                    filter["value"] = new JArray("all");
                                }
                                else
                                {
                                    filter["value"] = new JArray();
                                }
                            }
                        }

                        updated.Add(new
                        {
                            Filters = filters.ToJson(),
                            Id = id
                        });
                    }
                }
            }

            var updateSql = "UPDATE \"CustomFilters\" SET \"Filters\" = @Filters WHERE \"Id\" = @Id";
            conn.Execute(updateSql, updated, transaction: tran);
        }
    }
}
