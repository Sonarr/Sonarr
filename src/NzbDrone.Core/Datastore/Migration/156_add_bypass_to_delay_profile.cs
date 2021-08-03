using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(156)]
    public class add_bypass_to_delay_profile : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("DelayProfiles").AddColumn("BypassIfHighestQuality").AsBoolean().WithDefaultValue(false);

            // Set to true for existing Delay Profiles to keep behavior the same.
            Execute.Sql("UPDATE DelayProfiles SET BypassIfHighestQuality = 1;");
        }
    }
}
