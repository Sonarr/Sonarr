using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(239)]
    public class series_episode_retention : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                .AddColumn("RetentionEnabled").AsBoolean().NotNullable().WithDefaultValue(false)
                .AddColumn("RetentionKeepEpisodes").AsInt32().NotNullable().WithDefaultValue(5)
                .AddColumn("RetentionIncludeSpecials").AsBoolean().NotNullable().WithDefaultValue(false);
        }
    }
}
