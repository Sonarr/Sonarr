using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(208)]
    public class metadata_translations : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ImportLists").AddColumn("Language").AsInt32().Nullable().WithDefaultValue(1);
            Alter.Table("Series").AddColumn("Language").AsInt32().Nullable().WithDefaultValue(1);

            Alter.Table("Series").AddColumn("OriginalTitle").AsString().Nullable();
            Alter.Table("Series").AddColumn("CleanOriginalTitle").AsString().Nullable();
            Alter.Table("Series").AddColumn("SeasonType").AsString().Nullable().WithDefaultValue("official");
            Alter.Table("Series").AddColumn("SeasonTypes").AsString().Nullable().WithDefaultValue("[]");

            Create.Index().OnTable("Series").OnColumn("CleanOriginalTitle");

            Create.TableForModel("SeriesTranslations")
                .WithColumn("SeriesId").AsInt32()
                .WithColumn("Title").AsString().Nullable()
                .WithColumn("CleanTitle").AsString().Nullable()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Language").AsInt32();

            Create.Index().OnTable("SeriesTranslations").OnColumn("SeriesId");
        }
    }
}
