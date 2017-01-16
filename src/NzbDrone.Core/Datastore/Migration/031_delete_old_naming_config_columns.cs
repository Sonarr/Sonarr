using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(31)]
    public class delete_old_naming_config_columns : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Separator")
                  .Column("NumberStyle")
                  .Column("IncludeSeriesTitle")
                  .Column("IncludeEpisodeTitle")
                  .Column("IncludeQuality")
                  .Column("ReplaceSpaces")
                  .FromTable("NamingConfig");
        }
    }
}
