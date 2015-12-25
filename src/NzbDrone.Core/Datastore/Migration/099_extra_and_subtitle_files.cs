using System;
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

            // Set Extension using the extension from RelativePath
            Execute.Sql("UPDATE MetadataFiles SET Extension = substr(RelativePath, instr(RelativePath, '.'));");

            Alter.Table("MetadataFiles").AlterColumn("Extension").AsString().NotNullable();
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
