using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(87)]
    public class additonal_delayedprofiles_columns : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("DelayProfiles")
                 .AddColumn("EnableFilehoster").AsBoolean().WithDefaultValue(true).NotNullable()
                 .AddColumn("FilehosterDelay").AsInt32().WithDefaultValue(0).NotNullable();
        }
    }
}
