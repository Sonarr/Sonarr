using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(144)]
    public class import_lists_series_type_and_season_folder : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ImportLists").AddColumn("SeriesType").AsInt32().WithDefaultValue(0);
            Alter.Table("ImportLists").AddColumn("SeasonFolder").AsBoolean().WithDefaultValue(true);
        }
    }
}
