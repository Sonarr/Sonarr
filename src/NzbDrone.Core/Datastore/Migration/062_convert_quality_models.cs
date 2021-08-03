using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(62)]
    public class convert_quality_models : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertQualityModels);
        }

        private void ConvertQualityModels(IDbConnection conn, IDbTransaction tran)
        {
            ConvertQualityModelsOnTable(conn, tran, "EpisodeFiles");
            ConvertQualityModelsOnTable(conn, tran, "Blacklist");
            ConvertQualityModelsOnTable(conn, tran, "History");
        }

        private void ConvertQualityModelsOnTable(IDbConnection conn, IDbTransaction tran, string tableName)
        {
            var qualitiesToUpdate = new Dictionary<string, string>();

            using (IDbCommand qualityModelCmd = conn.CreateCommand())
            {
                qualityModelCmd.Transaction = tran;
                qualityModelCmd.CommandText = @"SELECT Distinct Quality FROM " + tableName;

                using (IDataReader qualityModelReader = qualityModelCmd.ExecuteReader())
                {
                    while (qualityModelReader.Read())
                    {
                        var qualityJson = qualityModelReader.GetString(0);

                        LegacyQualityModel062 quality;

                        if (!Json.TryDeserialize<LegacyQualityModel062>(qualityJson, out quality))
                        {
                            continue;
                        }

                        var newQualityModel = new QualityModel062 { Quality = quality.Quality, Revision = new Revision() };
                        if (quality.Proper)
                        {
                            newQualityModel.Revision.Version = 2;
                        }

                        var newQualityJson = newQualityModel.ToJson();

                        qualitiesToUpdate.Add(qualityJson, newQualityJson);
                    }
                }
            }

            foreach (var quality in qualitiesToUpdate)
            {
                using (IDbCommand updateCmd = conn.CreateCommand())
                {
                    updateCmd.Transaction = tran;
                    updateCmd.CommandText = "UPDATE " + tableName + " SET Quality = ? WHERE Quality = ?";
                    updateCmd.AddParameter(quality.Value);
                    updateCmd.AddParameter(quality.Key);

                    updateCmd.ExecuteNonQuery();
                }
            }
        }

        private class LegacyQualityModel062
        {
            public int Quality { get; set; }
            public bool Proper { get; set; }
        }

        private class QualityModel062
        {
            public int Quality { get; set; }
            public Revision Revision { get; set; }
        }
    }
}
