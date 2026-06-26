using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(231)]
    public class add_translations : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                 .AddColumn("Translations").AsString().Nullable();

            Alter.Table("Episodes")
                 .AddColumn("Translations").AsString().Nullable();
        }
    }
}
