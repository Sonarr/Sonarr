using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(11)]
    public class remove_ignored : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Ignored").FromTable("Seasons");
            Delete.Column("Ignored").FromTable("Episodes");
        }
    }
}
