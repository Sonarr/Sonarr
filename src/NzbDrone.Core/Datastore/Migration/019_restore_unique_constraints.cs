using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(19)]
    public class restore_unique_constraints : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // During an earlier version of drone, the indexes weren't recreated during alter table.
            Execute.Sql("DROP INDEX IF EXISTS \"IX_Series_TvdbId\"");
            Execute.Sql("DROP INDEX IF EXISTS \"IX_Series_TitleSlug\"");
            Execute.Sql("DROP INDEX IF EXISTS \"IX_Episodes_TvDbEpisodeId\"");

            Create.Index().OnTable("Series").OnColumn("TvdbId").Unique();
            Create.Index().OnTable("Series").OnColumn("TitleSlug").Unique();
            Create.Index().OnTable("Episodes").OnColumn("TvDbEpisodeId").Unique();
        }
    }
}
