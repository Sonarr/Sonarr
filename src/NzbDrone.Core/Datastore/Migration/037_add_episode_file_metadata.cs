using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(37)]
    public class add_episode_file_metadata : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("EpisodeFileMetaData")
                .WithColumn("SeriesId").AsInt32().NotNullable()
                .WithColumn("EpisodeFileId").AsInt32().NotNullable()
                .WithColumn("Provider").AsString().NotNullable()
                .WithColumn("Type").AsInt32().NotNullable()
                .WithColumn("LastUpdated").AsDateTime().NotNullable()
        }
    }
}
