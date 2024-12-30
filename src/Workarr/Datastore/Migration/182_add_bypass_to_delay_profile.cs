using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(182)]
    public class add_custom_format_score_bypass_to_delay_profile : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("DelayProfiles").AddColumn("BypassIfAboveCustomFormatScore").AsBoolean().WithDefaultValue(false);
            Alter.Table("DelayProfiles").AddColumn("MinimumCustomFormatScore").AsInt32().Nullable();
        }
    }
}
