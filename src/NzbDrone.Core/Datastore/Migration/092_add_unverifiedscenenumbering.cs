using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(92)]
    public class add_unverifiedscenenumbering : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("UnverifiedSceneNumbering").AsBoolean().WithDefaultValue(false);
        }
    }
}
