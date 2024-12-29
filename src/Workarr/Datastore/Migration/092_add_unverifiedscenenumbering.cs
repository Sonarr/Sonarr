using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
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
