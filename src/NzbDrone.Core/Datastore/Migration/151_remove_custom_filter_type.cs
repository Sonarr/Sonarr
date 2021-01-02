using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(151)]
    public class remove_custom_filter_type : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("CustomFilters").Set(new { Type = "series" }).Where(new { Type = "seriesIndex" });
            Update.Table("CustomFilters").Set(new { Type = "series" }).Where(new { Type = "seriesEditor" });
            Update.Table("CustomFilters").Set(new { Type = "series" }).Where(new { Type = "seasonPass" });
        }
    }
}
