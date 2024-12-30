using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
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
