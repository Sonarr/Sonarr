using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
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
