using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System.Data;
using System.Linq;
using NzbDrone.Common.Serializer;
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
            Alter.Table("QualityProfiles").AddColumn("Items").AsString().Nullable();

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

                        var items = Quality.DefaultQualityDefinitions.OrderBy(v => v.Weight).Select(v => new QualityProfileItem { Quality = v.Quality, Allowed = allowed.Contains(v.Quality) }).ToList();

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
                qualityModelCmd.CommandText = @"SELECT Id, Quality FROM " + tableName;
                using (IDataReader qualityModelReader = qualityModelCmd.ExecuteReader())
                {
                    while (qualityModelReader.Read())
                    {
                        var id = qualityModelReader.GetInt32(0);
                        var qualityJson = qualityModelReader.GetString(1);

                        var quality = Json.Deserialize<QualityModel>(qualityJson);

                        var qualityNewJson = qualityModelConverter.ToDB(quality);

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE " + tableName + " SET Quality = ? WHERE Id = ?";
                            updateCmd.AddParameter(qualityNewJson);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
