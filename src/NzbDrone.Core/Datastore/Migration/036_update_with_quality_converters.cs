using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System.Data;
using System.Linq;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using System.Collections.Generic;
using NzbDrone.Core.Datastore.Converters;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(36)]
    public class update_with_quality_converters : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            if (!Schema.Table("QualityProfiles").Column("Items").Exists())
            {
                Alter.Table("QualityProfiles").AddColumn("Items").AsString().Nullable();
            }

            Execute.WithConnection(ConvertQualityProfiles);
            Execute.WithConnection(ConvertQualityModels);
        }
        
        private void ConvertQualityProfiles(IDbConnection conn, IDbTransaction tran)
        {
            var qualityProfileItemConverter = new EmbeddedDocumentConverter(new QualityIntConverter());

            // Convert 'Allowed' column in QualityProfiles from Json List<object> to Json List<int> (int = Quality)
            using (IDbCommand qualityProfileCmd = conn.CreateCommand())
            {
                qualityProfileCmd.Transaction = tran;
                qualityProfileCmd.CommandText = @"SELECT Id, Allowed FROM QualityProfiles";
                using (IDataReader qualityProfileReader = qualityProfileCmd.ExecuteReader())
                {
                    while (qualityProfileReader.Read())
                    {
                        var id = qualityProfileReader.GetInt32(0);
                        var allowedJson = qualityProfileReader.GetString(1);

                        var allowed = Json.Deserialize<List<Quality>>(allowedJson);

                        var items = Quality.DefaultQualityDefinitions.OrderBy(v => v.Weight).Select(v => new ProfileQualityItem { Quality = v.Quality, Allowed = allowed.Contains(v.Quality) }).ToList();

                        var allowedNewJson = qualityProfileItemConverter.ToDB(items);

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE QualityProfiles SET Items = ? WHERE Id = ?";
                            updateCmd.AddParameter(allowedNewJson);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private void ConvertQualityModels(IDbConnection conn, IDbTransaction tran)
        {
            // Converts the QualityModel JSON objects to their new format (only storing the QualityId instead of the entire object)
            ConvertQualityModel(conn, tran, "Blacklist");
            ConvertQualityModel(conn, tran, "EpisodeFiles");
            ConvertQualityModel(conn, tran, "History");
        }

        private void ConvertQualityModel(IDbConnection conn, IDbTransaction tran, string tableName)
        {
            var qualityModelConverter = new EmbeddedDocumentConverter(new QualityIntConverter());

            using (IDbCommand qualityModelCmd = conn.CreateCommand())
            {
                qualityModelCmd.Transaction = tran;
                qualityModelCmd.CommandText = @"SELECT Distinct Quality FROM " + tableName;
                using (IDataReader qualityModelReader = qualityModelCmd.ExecuteReader())
                {
                    while (qualityModelReader.Read())
                    {
                        var qualityJson = qualityModelReader.GetString(0);

                        SourceQualityModel036 sourceQuality;

                        if (!Json.TryDeserialize<SourceQualityModel036>(qualityJson, out sourceQuality))
                        {
                            continue;
                        }

                        var qualityNewJson = qualityModelConverter.ToDB(new DestinationQualityModel036
                                                                        {
                                                                            Quality = sourceQuality.Quality.Id,
                                                                            Proper = sourceQuality.Proper
                                                                        });

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE " + tableName + " SET Quality = ? WHERE Quality = ?";
                            updateCmd.AddParameter(qualityNewJson);
                            updateCmd.AddParameter(qualityJson);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private class DestinationQualityModel036
        {
            public int Quality { get; set; }
            public bool Proper { get; set; }
        }

        private class SourceQualityModel036
        {
            public SourceQuality036 Quality { get; set; }
            public bool Proper { get; set; }
        }

        private class SourceQuality036
        {
            public int Id { get; set; }
        }
    }
}
