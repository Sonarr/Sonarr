using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(64)]
    public class remove_method_from_logs : NzbDroneMigrationBase
    {
        protected override void LogDbUpgrade()
        {
            Delete.Column("Method").FromTable("Logs");
        }
    }
}
