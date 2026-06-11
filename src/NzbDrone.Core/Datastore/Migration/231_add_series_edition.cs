using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(231)]
    public class add_series_edition : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                 .AddColumn("SeriesEdition").AsString().NotNullable().WithDefaultValue(SeriesEditions.Standard);

            Execute.Sql("DROP INDEX IF EXISTS \"IX_Series_TvdbId\"");
            Execute.Sql("DROP INDEX IF EXISTS \"IX_Series_TvdbId_SeriesEdition\"");

            Alter.Table("Series")
                 .AlterColumn("TvdbId").AsInt32();

            Create.Index("IX_Series_TvdbId_SeriesEdition")
                  .OnTable("Series")
                  .OnColumn("TvdbId").Ascending()
                  .OnColumn("SeriesEdition").Ascending()
                  .WithOptions().Unique();
        }
    }
}
