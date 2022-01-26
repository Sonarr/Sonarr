using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(70)]
    public class delay_profile : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("DelayProfiles")
                  .WithColumn("EnableUsenet").AsBoolean().NotNullable()
                  .WithColumn("EnableTorrent").AsBoolean().NotNullable()
                  .WithColumn("PreferredProtocol").AsInt32().NotNullable()
                  .WithColumn("UsenetDelay").AsInt32().NotNullable()
                  .WithColumn("TorrentDelay").AsInt32().NotNullable()
                  .WithColumn("Order").AsInt32().NotNullable()
                  .WithColumn("Tags").AsString().NotNullable();

            Insert.IntoTable("DelayProfiles").Row(new
                                                  {
                                                      EnableUsenet = true,
                                                      EnableTorrent = true,
                                                      PreferredProtocol = 1,
                                                      UsenetDelay = 0,
                                                      TorrentDelay = 0,
                                                      Order = int.MaxValue,
                                                      Tags = "[]"
                                                  });

            Execute.WithConnection(ConvertProfile);

            Delete.Column("GrabDelay").FromTable("Profiles");
            Delete.Column("GrabDelayMode").FromTable("Profiles");
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var profiles = GetProfiles(conn, tran);
            var order = 1;
            var updateProfiles = new List<object>();

            foreach (var profileClosure in profiles.DistinctBy(p => p.GrabDelay))
            {
                var profile = profileClosure;
                if (profile.GrabDelay == 0)
                {
                    continue;
                }

                var tag = string.Format("delay-{0}", profile.GrabDelay);
                var tagId = InsertTag(conn, tran, tag);
                var tags = string.Format("[{0}]", tagId);

                updateProfiles.Add(new
                {
                    UsenetDelay = profile.GrabDelay,
                    Order = order,
                    Tags = tags
                });

                var matchingProfileIds = profiles.Where(p => p.GrabDelay == profile.GrabDelay)
                                                 .Select(p => p.Id);

                UpdateSeries(conn, tran, matchingProfileIds, tagId);

                order++;
            }

            var insertDelayProfilesSql = $"INSERT INTO \"DelayProfiles\" (\"EnableUsenet\", \"EnableTorrent\", \"PreferredProtocol\", \"TorrentDelay\", \"UsenetDelay\", \"Order\", \"Tags\") VALUES (true, true, 1, 0, @UsenetDelay, @Order, @Tags)";
            conn.Execute(insertDelayProfilesSql, updateProfiles, transaction: tran);
        }

        private List<Profile69> GetProfiles(IDbConnection conn, IDbTransaction tran)
        {
            var profiles = new List<Profile69>();

            using (var getProfilesCmd = conn.CreateCommand())
            {
                getProfilesCmd.Transaction = tran;
                getProfilesCmd.CommandText = "SELECT \"Id\", \"GrabDelay\" FROM \"Profiles\"";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        var id = profileReader.GetInt32(0);
                        var delay = profileReader.GetInt32(1);

                        profiles.Add(new Profile69
                        {
                            Id = id,
                            GrabDelay = delay * 60
                        });
                    }
                }
            }

            return profiles;
        }

        private int InsertTag(IDbConnection conn, IDbTransaction tran, string tagLabel)
        {
            var parameters = new
            {
                TagLabel = tagLabel
            };

            var insertTagSql = "INSERT INTO \"Tags\" (\"Label\") VALUES (@TagLabel)";
            conn.Execute(insertTagSql, parameters, transaction: tran);

            var selectTagSql = "SELECT \"Id\" FROM \"Tags\" WHERE \"Label\" = @TagLabel";
            var id = conn.ExecuteScalar(selectTagSql, parameters, transaction: tran);

            return Convert.ToInt32(id);
        }

        private void UpdateSeries(IDbConnection conn, IDbTransaction tran, IEnumerable<int> profileIds, int tagId)
        {
            var updatedSeries = new List<object>();

            using (var getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = $"SELECT \"Id\", \"Tags\" FROM \"Series\" WHERE \"ProfileId\" IN ({string.Join(",", profileIds)})";

                using (var seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var id = seriesReader.GetInt32(0);
                        var tagString = seriesReader.GetString(1);

                        var tags = Json.Deserialize<List<int>>(tagString);
                        tags.Add(tagId);

                        updatedSeries.Add(new
                        {
                            Tags = tags.ToJson(),
                            Id = id
                        });
                    }
                }
            }

            var updateSeriesSql = "UPDATE \"Series\" SET \"Tags\" = @Tags WHERE \"Id\" = @Id";
            conn.Execute(updateSeriesSql, updatedSeries, transaction: tran);
        }
    }

    public class Profile69
    {
        public int Id { get; set; }
        public int GrabDelay { get; set; }
    }

    public class Series69
    {
        public int Id { get; set; }
        public List<int> Tags { get; set; }
        public DateTime? LastInfoSync { get; set; }
    }

    public class Tag69
    {
        public int Id { get; set; }
        public string Label { get; set; }
    }

    public class DelayProfile70
    {
        public int Id { get; set; }
        public bool EnableUsenet { get; set; }
        public bool EnableTorrent { get; set; }
        public int PreferredProtocol { get; set; }
        public int UsenetDelay { get; set; }
        public int TorrentDelay { get; set; }
        public int Order { get; set; }
        public List<int> Tags { get; set; }
    }
}
