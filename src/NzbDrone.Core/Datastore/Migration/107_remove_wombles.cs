using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(107)]
    public class remove_wombles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Indexers").Row(new { Implementation = "Wombles" });
        }
    }
}
