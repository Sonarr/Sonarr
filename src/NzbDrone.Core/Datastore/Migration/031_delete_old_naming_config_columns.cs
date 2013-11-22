using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(31)]
    public class delete_old_naming_config_columns : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            SqLiteAlter.DropColumns("NamingConfig",
                new[]
                {
                    "Separator",
                    "NumberStyle",
                    "IncludeSeriesTitle",
                    "IncludeEpisodeTitle",
                    "IncludeQuality",
                    "ReplaceSpaces"
                });
        }
    }
}
