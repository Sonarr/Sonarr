using System.Collections.Generic;
using System.Data;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(234)]
    public class pending_releases_season_numbers : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(MoveSeasonNumberToSeasonNumbers);
        }

        private void MoveSeasonNumberToSeasonNumbers(IDbConnection conn, IDbTransaction tran)
        {
            var updated = new List<object>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"ParsedEpisodeInfo\" FROM \"PendingReleases\"";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var parsedEpisodeInfo = Json.Deserialize<JObject>(reader.GetString(1));

                        if (!parsedEpisodeInfo.TryGetValue("seasonNumber", out var seasonNumber))
                        {
                            continue;
                        }

                        if (parsedEpisodeInfo["seasonNumbers"] == null)
                        {
                            parsedEpisodeInfo["seasonNumbers"] = seasonNumber.Type == JTokenType.Integer
                                ? new JArray(seasonNumber.Value<int>())
                                : new JArray();
                        }

                        parsedEpisodeInfo.Remove("seasonNumber");

                        updated.Add(new
                        {
                            ParsedEpisodeInfo = parsedEpisodeInfo.ToJson(),
                            Id = id
                        });
                    }
                }
            }

            var updateSql = "UPDATE \"PendingReleases\" SET \"ParsedEpisodeInfo\" = @ParsedEpisodeInfo WHERE \"Id\" = @Id";
            conn.Execute(updateSql, updated, transaction: tran);
        }
    }
}
