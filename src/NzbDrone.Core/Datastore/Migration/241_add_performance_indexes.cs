using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(241)]
    public class add_performance_indexes : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Index().OnTable("Episodes").OnColumn("AirDateUtc");
            Create.Index().OnTable("ExtraFiles").OnColumn("SeriesId");
            Create.Index().OnTable("ExtraFiles").OnColumn("EpisodeFileId");
            Create.Index().OnTable("SubtitleFiles").OnColumn("SeriesId");
            Create.Index().OnTable("SubtitleFiles").OnColumn("EpisodeFileId");
        }
    }
}
