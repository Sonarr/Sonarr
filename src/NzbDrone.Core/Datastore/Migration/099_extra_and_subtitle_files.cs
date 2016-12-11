using System;
using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(99)]
    public class extra_and_subtitle_files : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("ExtraFiles")
                  .WithColumn("SeriesId").AsInt32().NotNullable()
                  .WithColumn("SeasonNumber").AsInt32().NotNullable()
                  .WithColumn("EpisodeFileId").AsInt32().NotNullable()
                  .WithColumn("RelativePath").AsString().NotNullable()
                  .WithColumn("Extension").AsString().NotNullable()
                  .WithColumn("Added").AsDateTime().NotNullable()
                  .WithColumn("LastUpdated").AsDateTime().NotNullable();

            Create.TableForModel("SubtitleFiles")
                  .WithColumn("SeriesId").AsInt32().NotNullable()
                  .WithColumn("SeasonNumber").AsInt32().NotNullable()
                  .WithColumn("EpisodeFileId").AsInt32().NotNullable()
                  .WithColumn("RelativePath").AsString().NotNullable()
                  .WithColumn("Extension").AsString().NotNullable()
                  .WithColumn("Added").AsDateTime().NotNullable()
                  .WithColumn("LastUpdated").AsDateTime().NotNullable()
                  .WithColumn("Language").AsInt32().NotNullable();

            Alter.Table("MetadataFiles")
                 .AddColumn("Added").AsDateTime().Nullable()
                 .AddColumn("Extension").AsString().Nullable();

            // Remove Metadata files that don't have an extension
            Execute.Sql("DELETE FROM MetadataFiles WHERE RelativePath NOT LIKE '%.%'");

            // Set Extension using the extension from RelativePath
            Execute.WithConnection(SetMetadataFileExtension);

            Alter.Table("MetadataFiles").AlterColumn("Extension").AsString().NotNullable();
        }

        private void SetMetadataFileExtension(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Id, RelativePath FROM MetadataFiles";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var relativePath = reader.GetString(1);
                        var extension = relativePath.Substring(relativePath.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase));

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE MetadataFiles SET Extension = ? WHERE Id = ?";
                            updateCmd.AddParameter(extension);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }

    public class MetadataFile99
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public int? EpisodeFileId { get; set; }
        public int? SeasonNumber { get; set; }
        public string RelativePath { get; set; }
        public DateTime Added { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Extension { get; set; }
        public string Hash { get; set; }
        public string Consumer { get; set; }
        public int Type { get; set; }
    }
}
