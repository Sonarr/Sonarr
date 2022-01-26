using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(87)]
    public class remove_eztv : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Indexers").Row(new { Implementation = "Eztv" });
        }
    }
}
