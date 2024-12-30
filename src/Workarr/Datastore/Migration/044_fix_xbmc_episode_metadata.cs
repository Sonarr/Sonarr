using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(44)]
    public class fix_xbmc_episode_metadata : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Convert Episode Metadata to proper type
            Execute.Sql("UPDATE \"MetadataFiles\" " +
                        "SET \"Type\" = 2 " +
                        "WHERE \"Consumer\" = 'XbmcMetadata' " +
                        "AND \"EpisodeFileId\" IS NOT NULL " +
                        "AND \"Type\" = 4 " +
                        "AND \"RelativePath\" LIKE '%.nfo'");

            // Convert Episode Images to proper type
            Execute.Sql("UPDATE \"MetadataFiles\" " +
                        "SET \"Type\" = 5 " +
                        "WHERE \"Consumer\" = 'XbmcMetadata' " +
                        "AND \"EpisodeFileId\" IS NOT NULL " +
                        "AND \"Type\" = 4");
        }
    }
}
