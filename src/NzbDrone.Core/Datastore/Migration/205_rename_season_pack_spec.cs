using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(205)]
    public class rename_season_pack_spec : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"CustomFormats\" SET \"Specifications\" = REPLACE(\"Specifications\", 'SeasonPackSpecification', 'ReleaseTypeSpecification')");
        }
    }
}
