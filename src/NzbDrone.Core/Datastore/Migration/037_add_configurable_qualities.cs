using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(37)]
    public class add_configurable_qualities : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Allowed").FromTable("QualityProfiles");

            Alter.Column("Items").OnTable("QualityProfiles").AsString().NotNullable();

            Create.TableForModel("QualityDefinitions")
                    .WithColumn("Quality").AsInt32().Unique()
                    .WithColumn("Title").AsString().Unique()
                    .WithColumn("Weight").AsInt32().Unique()
                    .WithColumn("MinSize").AsInt32()
                    .WithColumn("MaxSize").AsInt32();

            Execute.WithConnection(ConvertQualities);

            Delete.Table("QualitySizes");
        }

        private void ConvertQualities(IDbConnection conn, IDbTransaction tran)
        {
            // Convert QualitySizes to a more generic QualityDefinitions table.
            using (IDbCommand qualitySizeCmd = conn.CreateCommand())
            {
                qualitySizeCmd.Transaction = tran;
                qualitySizeCmd.CommandText = @"SELECT QualityId, MinSize, MaxSize FROM QualitySizes";
                using (IDataReader qualitySizeReader = qualitySizeCmd.ExecuteReader())
                {
                    while (qualitySizeReader.Read())
                    {
                        var qualityId = qualitySizeReader.GetInt32(0);
                        var minSize = qualitySizeReader.GetInt32(1);
                        var maxSize = qualitySizeReader.GetInt32(2);

                        var defaultConfig = Quality.DefaultQualityDefinitions.Single(p => (int)p.Quality == qualityId);

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "INSERT INTO QualityDefinitions (Quality, Title, Weight, MinSize, MaxSize) VALUES (?, ?, ?, ?, ?)";
                            updateCmd.AddParameter(qualityId);
                            updateCmd.AddParameter(defaultConfig.Title);
                            updateCmd.AddParameter(defaultConfig.Weight);
                            updateCmd.AddParameter(minSize);
                            updateCmd.AddParameter(maxSize);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
