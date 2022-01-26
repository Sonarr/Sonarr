using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(183)]
    public class update_images_remote_url : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Episodes\" SET \"Images\" = REPLACE(\"Images\", '\"url\"', '\"remoteUrl\"')");
            Execute.Sql("UPDATE \"Series\" SET \"Images\" = REPLACE(\"Images\", '\"url\"', '\"remoteUrl\"'), \"Actors\" = REPLACE(\"Actors\", '\"url\"', '\"remoteUrl\"'), \"Seasons\" = REPLACE(\"Seasons\", '\"url\"', '\"remoteUrl\"')");
        }
    }
}
