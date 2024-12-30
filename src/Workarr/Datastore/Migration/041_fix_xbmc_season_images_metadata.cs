using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(41)]
    public class fix_xbmc_season_images_metadata : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"MetadataFiles\" SET \"Type\" = 4 WHERE \"Consumer\" = 'XbmcMetadata' AND \"SeasonNumber\" IS NOT NULL");
        }
    }
}
